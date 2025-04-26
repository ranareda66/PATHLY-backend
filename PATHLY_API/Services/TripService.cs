using PATHLY_API.Models;
using PATHLY_API.Dto;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PATHLY_API.Data;
using PATHLY_API.Models.Enums;

namespace PATHLY_API.Services
{
    public class TripService
    {
        private const int FREE_TRIPS_LIMIT = 5;
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

                // Get basic route information (always available)
                var route = await _googleTripService.GetRouteBetweenCoordinatesAsync(
                    startLat, startLng, endLat, endLng);

                // Check road prediction access
                var (canAccessRoadPrediction, freeTripsRemaining) =
                    await CheckRoadPredictionAccess(user, requestRoadPrediction);

                List<ClusterGroup> roadPrediction = null;
                if (requestRoadPrediction && canAccessRoadPrediction)
                {
                    roadPrediction = await _roadPredictionService.PredictRoadConditionAsync(
                        startLat, startLng, endLat, endLng);

                    if (!await HasActiveSubscription(userId.Value))
                    {
                        user.FreeTripsUsed++;
                        _logger.LogInformation("User {UserId} used free trip ({Remaining} remaining)",
                            userId, FREE_TRIPS_LIMIT - user.FreeTripsUsed);
                    }
                }

                // Create and save trip
                var trip = new Trip
                {
                    UserId = userId.Value,
                    StartLatitude = startLat,
                    StartLongitude = startLng,
                    EndLatitude = endLat,
                    EndLongitude = endLng,
                    Distance = route.DistanceKm,
                    RoutePolyline = route.Polyline,
                    StartTime = DateTime.UtcNow,
                    Status = TripStatus.InProgress,
                    UsedRoadPrediction = requestRoadPrediction && canAccessRoadPrediction
                };

                user.TripCount++;
                _context.Trips.Add(trip);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new TripResultDto
                {
                    Trip = MapToTripDto(trip),
                    Route = MapToRouteDto(route),
                    RoadPrediction = roadPrediction,
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

        private async Task<(bool canAccess, int? freeTripsRemaining)> CheckRoadPredictionAccess(
            User user, bool requestRoadPrediction)
        {
            if (!requestRoadPrediction)
                return (false, null);

            if (await HasActiveSubscription(user.Id))
                return (true, null); // Subscribed users have unlimited access

            int remaining = FREE_TRIPS_LIMIT - user.FreeTripsUsed;
            return (remaining > 0, remaining > 0 ? remaining : (int?)null);
        }

        private async Task<bool> HasActiveSubscription(int userId)
        {
            return await _context.UserSubscriptions
                .AnyAsync(us => us.UserId == userId &&
                               us.Status == SubscriptionStatus.Active &&
                               us.EndDate > DateTime.UtcNow);
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
                StartLatitude = trip.StartLatitude,
                StartLongitude = trip.StartLongitude,
                EndLatitude = trip.EndLatitude,
                EndLongitude = trip.EndLongitude,
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

        public async Task<List<TripDto>> GetUserTripsAsync(
            int userId,
            DateTime? startDate = null,
            TripStatus? status = null)
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
                .Select(t => MapToTripDto(t))
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

            return new TripResponseDto
            {
                Id = trip.Id,
                StartLatitude = trip.StartLatitude,
                StartLongitude = trip.StartLongitude,
                EndLatitude = trip.EndLatitude,
                EndLongitude = trip.EndLongitude,
                Distance = trip.Distance,
                RoutePolyline = trip.RoutePolyline,
                StartTime = trip.StartTime,
                EndTime = trip.EndTime,
                Status = trip.Status,
                FeedbackRate = trip.FeedbackRate,
                UsedRoadPrediction = trip.UsedRoadPrediction
            };
        }

    }
}