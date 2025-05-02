using PATHLY_API.Dto;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PATHLY_API.Data;
using PATHLY_API.Interfaces;

namespace PATHLY_API.Services
{
    public class TripService : ITripService
    {
        public const int FREE_TRIPS_LIMIT = 5;
        private const double HAZARD_SEVERITY_DISTANCE = 25;
        private const double WARNING_RANGE = 1000; // 1000 meters for hazard notifications
        private const double MAX_DISTANCE_TO_ROUTE = 50; // Max distance (meters) user can be from route

        private readonly ApplicationDbContext _context;
        private readonly GoogleTripService _googleTripService;
        private readonly IRoadPredictionService _roadPredictionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TripService> _logger;

        public TripService(
            ApplicationDbContext context,
            GoogleTripService googleTripService,
            IRoadPredictionService roadPredictionService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TripService> logger)
        {
            _context = context;
            _googleTripService = googleTripService;
            _roadPredictionService = roadPredictionService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> HasActiveSubscription(int userId)
        {
            return await _context.UserSubscriptions
                .AnyAsync(us => us.UserId == userId &&
                              us.Status == SubscriptionStatus.Active &&
                              us.EndDate > DateTime.UtcNow);
        }

        public async Task<TripResultDto> StartTripWithCoordinatesAsync(
            double startLat, double startLng,
            double endLat, double endLng,
            bool requestRoadPrediction)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    throw new UnauthorizedAccessException("User not authenticated");

                var user = await _context.Users
                    .Include(u => u.UserSubscriptions)
                    .FirstOrDefaultAsync(u => u.Id == userId.Value)
                    ?? throw new KeyNotFoundException("User not found");

                var route = await _googleTripService.GetRouteBetweenCoordinatesAsync(
                    startLat, startLng, endLat, endLng);

                var (canAccessRoadPrediction, freeTripsRemaining) =
                    await CheckRoadPredictionAccess(user, requestRoadPrediction);

                List<ClusterGroup> roadPrediction = null;
                List<HazardNotificationDto> hazardNotifications = null;

                if (requestRoadPrediction && canAccessRoadPrediction)
                {
                    roadPrediction = await _roadPredictionService.PredictRoadConditionAsync(
                        startLat, startLng, endLat, endLng);

                    var tempNotifications = GenerateHazardNotifications(
                        roadPrediction,
                        route.Steps?.Select(s => new RouteStepDto
                        {
                            Instruction = s.Instruction,
                            Distance = s.Distance,
                            Duration = s.Duration,
                            StartLatitude = s.StartLocation.Latitude,
                            StartLongitude = s.StartLocation.Longitude,
                            EndLatitude = s.EndLocation.Latitude,
                            EndLongitude = s.EndLocation.Longitude
                        }).ToList(),
                        route.Polyline);

                    // Calculate userRouteDistance for start position
                    var routePoints = DecodePolyline(route.Polyline);
                    if (routePoints.Count < 2)
                    {
                        _logger.LogWarning($"Insufficient route points ({routePoints.Count}) to calculate hazards for start position");
                        hazardNotifications = tempNotifications; // Fallback to absolute distances
                    }
                    else
                    {
                        var (closestRoutePoint, userRouteDistance, distanceToRoute) = FindClosestRoutePoint(routePoints, startLat, startLng);
                        _logger.LogInformation($"Trip start position: ({startLat}, {startLng}), Closest point: ({closestRoutePoint.Lat}, {closestRoutePoint.Lng}), Route distance: {userRouteDistance:F2}m, Distance to route: {distanceToRoute:F2}m");

                        // Validate start position
                        if (distanceToRoute > MAX_DISTANCE_TO_ROUTE)
                        {
                            _logger.LogWarning($"Start position is {distanceToRoute:F2} meters from route, exceeding max allowed distance of {MAX_DISTANCE_TO_ROUTE} meters. Using absolute distances.");
                            hazardNotifications = tempNotifications;
                        }
                        else
                        {
                            // Adjust hazard distances to be relative to start position
                            hazardNotifications = tempNotifications
                                .Where(h => h.DistanceAhead >= userRouteDistance - 50) // Include hazards at or ahead of start
                                .Select(h => new HazardNotificationDto
                                {
                                    HazardType = h.HazardType,
                                    Latitude = h.Latitude,
                                    Longitude = h.Longitude,
                                    DistanceAhead = Math.Max(0, h.DistanceAhead - userRouteDistance),
                                    WarningMessage = h.WarningMessage
                                })
                                .OrderBy(h => h.DistanceAhead)
                                .ToList();
                        }
                    }

                    if (!await HasActiveSubscription(userId.Value))
                    {
                        user.FreeTripsUsed++;
                        await _context.SaveChangesAsync();
                    }
                }

                var trip = new Trip
                {
                    UserId = userId.Value,
                    StartLatitude = (decimal)startLat,
                    StartLongitude = (decimal)startLng,
                    EndLatitude = (decimal)endLat,
                    EndLongitude = (decimal)endLng,
                    Distance = route.DistanceKm,
                    RoutePolyline = route.Polyline,
                    StartTime = DateTime.UtcNow,
                    Status = TripStatus.InProgress,
                    UsedRoadPrediction = requestRoadPrediction && canAccessRoadPrediction
                };

                _context.Trips.Add(trip);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new TripResultDto
                {
                    Trip = MapToTripDto(trip),
                    Route = MapToRouteDto(route),
                    RoadPrediction = roadPrediction,
                    HazardNotifications = hazardNotifications,
                    FreeTripsRemaining = freeTripsRemaining,
                    RequiresSubscription = requestRoadPrediction && !canAccessRoadPrediction
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error starting trip");
                throw;
            }
        }

        public async Task<List<HazardNotificationDto>> GetUpcomingHazards(int tripId, double currentLat, double currentLng)
        {
            var trip = await _context.Trips
                .AsNoTracking()
                .Include(t => t.User)
                .Select(t => new
                {
                    t.Id,
                    t.UsedRoadPrediction,
                    t.RoutePolyline,
                    StartLat = (double)t.StartLatitude,
                    StartLng = (double)t.StartLongitude,
                    EndLat = (double)t.EndLatitude,
                    EndLng = (double)t.EndLongitude
                })
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null)
                throw new KeyNotFoundException("Trip not found");

            if (!trip.UsedRoadPrediction)
                throw new InvalidOperationException("This trip doesn't have hazard prediction enabled");

            var route = await _googleTripService.GetRouteBetweenCoordinatesAsync(
                trip.StartLat, trip.StartLng,
                trip.EndLat, trip.EndLng);

            var roadPrediction = await _roadPredictionService.PredictRoadConditionAsync(
                trip.StartLat, trip.StartLng,
                trip.EndLat, trip.EndLng);

            var allHazards = GenerateHazardNotifications(
                roadPrediction,
                route.Steps?.Select(s => new RouteStepDto
                {
                    Instruction = s.Instruction,
                    Distance = s.Distance,
                    Duration = s.Duration,
                    StartLatitude = s.StartLocation.Latitude,
                    StartLongitude = s.StartLocation.Longitude,
                    EndLatitude = s.EndLocation.Latitude,
                    EndLongitude = s.EndLocation.Longitude
                }).ToList(),
                trip.RoutePolyline);

            // Decode the route polyline to find the user's position
            var routePoints = DecodePolyline(trip.RoutePolyline);
            _logger.LogInformation($"Trip {tripId}: Decoded {routePoints.Count} points. Start: ({routePoints.FirstOrDefault().Lat}, {routePoints.FirstOrDefault().Lng}), End: ({routePoints.LastOrDefault().Lat}, {routePoints.LastOrDefault().Lng})");

            if (routePoints.Count < 2)
            {
                _logger.LogWarning($"Trip {tripId}: Insufficient route points ({routePoints.Count}) to calculate hazards");
                return new List<HazardNotificationDto>();
            }

            var (closestRoutePoint, userRouteDistance, distanceToRoute) = FindClosestRoutePoint(routePoints, currentLat, currentLng);
            _logger.LogInformation($"Trip {tripId}: User position: ({currentLat}, {currentLng}), Closest point: ({closestRoutePoint.Lat}, {closestRoutePoint.Lng}), Route distance: {userRouteDistance:F2}m, Distance to route: {distanceToRoute:F2}m");

            // Validate user position
            if (distanceToRoute > MAX_DISTANCE_TO_ROUTE)
            {
                _logger.LogWarning($"Trip {tripId}: User position is {distanceToRoute:F2} meters from route, exceeding max allowed distance of {MAX_DISTANCE_TO_ROUTE} meters");
                return new List<HazardNotificationDto>();
            }

            // Filter hazards that are ahead of the user and within the warning range
            var upcomingHazards = allHazards
                .Where(h =>
                {
                    var distanceFromUser = h.DistanceAhead - userRouteDistance;
                    var isAhead = distanceFromUser > -50; // Allow slight negative to handle inaccuracies
                    var isWithinRange = distanceFromUser <= WARNING_RANGE;
                    _logger.LogDebug($"Trip {tripId}: Hazard at ({h.Latitude}, {h.Longitude}), Type={h.HazardType}, DistanceAhead={h.DistanceAhead:F2}m, DistanceFromUser={distanceFromUser:F2}m, IsAhead={isAhead}, IsWithinRange={isWithinRange}");
                    return isAhead && isWithinRange;
                })
                .OrderBy(h => h.DistanceAhead)
                .Select(h => new HazardNotificationDto
                {
                    HazardType = h.HazardType,
                    Latitude = h.Latitude,
                    Longitude = h.Longitude,
                    DistanceAhead = Math.Max(0, h.DistanceAhead - userRouteDistance),
                    WarningMessage = h.WarningMessage
                })
                .ToList();

            _logger.LogInformation($"Trip {tripId}: Found {upcomingHazards.Count} upcoming hazards");
            return upcomingHazards;
        }

        public async Task<bool> EndTripAsync(int tripId, int? feedback = null)
        {
            var trip = await GetValidTripAsync(tripId);

            if (trip.Status != TripStatus.InProgress)
                throw new InvalidOperationException("Only in-progress trips can be ended");

            trip.EndTime = DateTime.UtcNow;
            trip.Status = TripStatus.Completed;

            if (feedback.HasValue)
            {
                if (feedback < 1 || feedback > 5)
                    throw new ArgumentOutOfRangeException(nameof(feedback), "Feedback must be between 1 and 5");
                trip.FeedbackRate = feedback.Value;
            }

            _context.Trips.Update(trip);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AbortTripAsync(int tripId)
        {
            var trip = await GetValidTripAsync(tripId);

            if (trip.Status != TripStatus.InProgress)
                throw new InvalidOperationException("Only in-progress trips can be aborted");

            trip.EndTime = DateTime.UtcNow;
            trip.Status = TripStatus.Cancelled;

            _context.Trips.Update(trip);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteTripAsync(int tripId)
        {
            var trip = await GetValidTripAsync(tripId);

            if (trip.Status != TripStatus.Cancelled)
                throw new InvalidOperationException("Only cancelled trips can be deleted");

            _context.Trips.Remove(trip);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<TripDto>> GetUserTripsAsync(int userId, DateTime? startDate = null, TripStatus? status = null)
        {
            var query = _context.Trips
                .Where(t => t.UserId == userId)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(t => t.StartTime >= startDate.Value);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            return await query
                .OrderByDescending(t => t.StartTime)
                .Select(t => new TripDto
                {
                    Id = t.Id,
                    StartLatitude = (double)t.StartLatitude,
                    StartLongitude = (double)t.StartLongitude,
                    EndLatitude = (double)t.EndLatitude,
                    EndLongitude = (double)t.EndLongitude,
                    Distance = t.Distance,
                    RoutePolyline = t.RoutePolyline,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    Status = t.Status.ToString(),
                    UsedRoadPrediction = t.UsedRoadPrediction
                })
                .ToListAsync();
        }

        public async Task<TripResponseDto> GetTripDetailsAsync(int tripId)
        {
            var trip = await _context.Trips
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == tripId)
                ?? throw new KeyNotFoundException("Trip not found");

            var userId = GetCurrentUserId();
            if (trip.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to view this trip");

            var route = await _googleTripService.GetRouteBetweenCoordinatesAsync(
                (double)trip.StartLatitude,
                (double)trip.StartLongitude,
                (double)trip.EndLatitude,
                (double)trip.EndLongitude);

            return new TripResponseDto
            {
                Id = trip.Id,
                StartLatitude = (double)trip.StartLatitude,
                StartLongitude = (double)trip.StartLongitude,
                EndLatitude = (double)trip.EndLatitude,
                EndLongitude = (double)trip.EndLongitude,
                Distance = trip.Distance,
                RoutePolyline = trip.RoutePolyline,
                StartTime = trip.StartTime,
                EndTime = trip.EndTime,
                Status = trip.Status,
                FeedbackRate = trip.FeedbackRate,
                UsedRoadPrediction = trip.UsedRoadPrediction,
                Route = MapToRouteDto(route)
            };
        }

        private async Task<Trip> GetValidTripAsync(int tripId)
        {
            var trip = await _context.Trips
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == tripId)
                ?? throw new KeyNotFoundException($"Trip with ID {tripId} not found");

            var userId = GetCurrentUserId();
            if (userId == null || trip.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to access this trip");

            return trip;
        }

        private List<HazardNotificationDto> GenerateHazardNotifications(
            List<ClusterGroup> roadPrediction,
            List<RouteStepDto> routeSteps,
            string routePolyline)
        {
            var notifications = new List<HazardNotificationDto>();

            if (roadPrediction == null || routeSteps == null || string.IsNullOrEmpty(routePolyline))
            {
                _logger.LogWarning("Invalid input for hazard notifications: roadPrediction={RoadPrediction}, routeSteps={RouteSteps}, routePolyline={RoutePolyline}",
                    roadPrediction == null ? "null" : "non-null",
                    routeSteps == null ? "null" : "non-null",
                    string.IsNullOrEmpty(routePolyline) ? "empty" : "non-empty");
                return notifications;
            }

            // Decode the route polyline into a list of coordinates
            var routePoints = DecodePolyline(routePolyline);
            _logger.LogInformation($"Hazard notifications: Decoded {routePoints.Count} points. Start: ({routePoints.FirstOrDefault().Lat}, {routePoints.FirstOrDefault().Lng}), End: ({routePoints.LastOrDefault().Lat}, {routePoints.LastOrDefault().Lng})");

            var hazards = roadPrediction.Where(g => g.ClusterName != "Good Road").ToList();

            foreach (var hazardGroup in hazards)
            {
                foreach (var hazardPoint in hazardGroup.Points)
                {
                    // Find the closest point on the route to the hazard
                    var (closestRoutePoint, routeDistance, distanceToRoute) = FindClosestRoutePoint(
                        routePoints, hazardPoint.Latitude, hazardPoint.Longitude);

                    // Only include hazards that are close to the route (e.g., within 50 meters)
                    if (distanceToRoute <= 50)
                    {
                        // Check for nearby hazards to avoid duplicates
                        var existingNotification = notifications.FirstOrDefault(n =>
                            n.HazardType == MapClusterToType(hazardGroup.ClusterName) &&
                            CalculateDistance(n.Latitude, n.Longitude,
                                hazardPoint.Latitude, hazardPoint.Longitude) < HAZARD_SEVERITY_DISTANCE);

                        if (existingNotification == null)
                        {
                            var notification = new HazardNotificationDto
                            {
                                HazardType = MapClusterToType(hazardGroup.ClusterName),
                                Latitude = hazardPoint.Latitude,
                                Longitude = hazardPoint.Longitude,
                                DistanceAhead = routeDistance,
                                WarningMessage = GetWarningMessage(hazardGroup.ClusterName)
                            };
                            notifications.Add(notification);
                            _logger.LogDebug($"Added hazard: Type={notification.HazardType}, Lat={notification.Latitude}, Lng={notification.Longitude}, DistanceAhead={notification.DistanceAhead:F2}m, DistanceToRoute={distanceToRoute:F2}m");
                        }
                    }
                    else
                    {
                        _logger.LogDebug($"Excluded hazard at ({hazardPoint.Latitude}, {hazardPoint.Longitude}) (distance to route: {distanceToRoute:F2}m)");
                    }
                }
            }

            return notifications.OrderBy(n => n.DistanceAhead).ToList();
        }

        private async Task<(bool canAccess, int? freeTripsRemaining)> CheckRoadPredictionAccess(
            User user, bool requestRoadPrediction)
        {
            if (!requestRoadPrediction)
                return (false, null);

            if (await HasActiveSubscription(user.Id))
                return (true, null);

            int remaining = FREE_TRIPS_LIMIT - user.FreeTripsUsed;
            return (remaining > 0, remaining > 0 ? remaining : (int?)null);
        }

        private string MapClusterToType(string clusterName)
        {
            return clusterName switch
            {
                "Bump" => "bump",
                "Hole" => "hole",
                "Bad Road" => "bad_road",
                _ => "unknown"
            };
        }

        private string GetWarningMessage(string clusterName)
        {
            return clusterName switch
            {
                "Bump" => "Speed bump ahead! Slow down.",
                "Hole" => "Pothole ahead! Be careful.",
                "Bad Road" => "Rough road ahead! Proceed with caution.",
                _ => "Road hazard ahead!"
            };
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Earth's radius in meters
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : null;
        }

        private TripDto MapToTripDto(Trip trip)
        {
            return new TripDto
            {
                Id = trip.Id,
                StartLatitude = (double)trip.StartLatitude,
                StartLongitude = (double)trip.StartLongitude,
                EndLatitude = (double)trip.EndLatitude,
                EndLongitude = (double)trip.EndLongitude,
                Distance = trip.Distance,
                RoutePolyline = trip.RoutePolyline,
                StartTime = trip.StartTime,
                EndTime = trip.EndTime,
                Status = trip.Status.ToString(),
                UsedRoadPrediction = trip.UsedRoadPrediction
            };
        }

        private RouteDto MapToRouteDto(GoogleTripService.RouteResult route)
        {
            return new RouteDto
            {
                Polyline = route.Polyline,
                Steps = route.Steps?.Select(s => new RouteStepDto
                {
                    Instruction = s.Instruction,
                    Distance = s.Distance,
                    Duration = s.Duration,
                    StartLatitude = s.StartLocation.Latitude,
                    StartLongitude = s.StartLocation.Longitude,
                    EndLatitude = s.EndLocation.Latitude,
                    EndLongitude = s.EndLocation.Longitude
                }).ToList(),
                DistanceKm = route.DistanceKm,
                Duration = route.Duration
            };
        }

        private (RoutePoint Point, double RouteDistance, double DistanceToRoute) FindClosestRoutePoint(
            List<(double Lat, double Lng)> routePoints, double targetLat, double targetLng)
        {
            if (routePoints.Count < 2)
            {
                _logger.LogWarning("Insufficient route points for projection: {Count}", routePoints.Count);
                var point = routePoints.FirstOrDefault();
                return (new RoutePoint { Lat = point.Lat, Lng = point.Lng }, 0, CalculateDistance(targetLat, targetLng, point.Lat, point.Lng));
            }

            RoutePoint closestPoint = null;
            double minDistance = double.MaxValue;
            double routeDistance = 0;
            double segmentDistance = 0;

            for (int i = 0; i < routePoints.Count - 1; i++)
            {
                var p1 = routePoints[i];
                var p2 = routePoints[i + 1];

                // Project the target point onto the segment p1-p2
                var (projLat, projLng, t) = ProjectPointToSegment(
                    p1.Lat, p1.Lng, p2.Lat, p2.Lng, targetLat, targetLng);

                // Calculate distance from target to projected point
                var distance = CalculateDistance(targetLat, targetLng, projLat, projLng);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = new RoutePoint { Lat = projLat, Lng = projLng };

                    // Calculate route distance to the start of the segment
                    routeDistance = 0;
                    for (int j = 1; j <= i; j++)
                    {
                        routeDistance += CalculateDistance(
                            routePoints[j - 1].Lat, routePoints[j - 1].Lng,
                            routePoints[j].Lat, routePoints[j].Lng);
                    }

                    // Add distance along the segment to the projected point
                    if (t > 0 && t < 1)
                    {
                        segmentDistance = t * CalculateDistance(p1.Lat, p1.Lng, p2.Lat, p2.Lng);
                    }
                    else
                    {
                        segmentDistance = t <= 0
                            ? 0
                            : CalculateDistance(p1.Lat, p1.Lng, p2.Lat, p2.Lng);
                    }
                    routeDistance += segmentDistance;
                }
            }

            return (closestPoint, routeDistance, minDistance);
        }

        private (double Lat, double Lng, double t) ProjectPointToSegment(
            double x1, double y1, double x2, double y2, double x, double y)
        {
            // Convert to radians for vector calculations
            double dx = ToRadians(x2 - x1);
            double dy = ToRadians(y2 - y1);
            double px = ToRadians(x - x1);
            double py = ToRadians(y - y1);

            double segmentLengthSquared = dx * dx + dy * dy;
            if (segmentLengthSquared == 0)
                return (x1, y1, 0);

            // Calculate the projection parameter t
            double t = Math.Max(0, Math.Min(1, (px * dx + py * dy) / segmentLengthSquared));

            // Calculate the projected point
            double projLat = x1 + t * (x2 - x1);
            double projLng = y1 + t * (y2 - y1);

            return (projLat, projLng, t);
        }

        private List<(double Lat, double Lng)> DecodePolyline(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
            {
                _logger.LogWarning("Empty polyline provided");
                return new List<(double Lat, double Lng)>();
            }

            try
            {
                var poly = new List<(double Lat, double Lng)>();
                int index = 0, len = encoded.Length;
                int lat = 0, lng = 0;

                while (index < len)
                {
                    int b, shift = 0, result = 0;
                    do
                    {
                        b = encoded[index++] - 63;
                        result |= (b & 0x1f) << shift;
                        shift += 5;
                    } while (b >= 0x20);
                    int dlat = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                    lat += dlat;

                    shift = 0;
                    result = 0;
                    do
                    {
                        b = encoded[index++] - 63;
                        result |= (b & 0x1f) << shift;
                        shift += 5;
                    } while (b >= 0x20);
                    int dlng = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                    lng += dlng;

                    var point = ((double)lat / 1E5, (double)lng / 1E5);
                    poly.Add(point);
                    _logger.LogDebug($"Decoded point: Lat={point.Item1:F6}, Lng={point.Item2:F6}");
                }

                _logger.LogInformation($"Decoded {poly.Count} points from polyline");
                return poly;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decode polyline: {Encoded}", encoded);
                return new List<(double Lat, double Lng)>();
            }
        }

        private class RoutePoint
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }
    }
}