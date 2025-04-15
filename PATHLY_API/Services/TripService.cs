using PATHLY_API.Data;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;

namespace PATHLY_API.Services
{
    public class TripService
    {
        private readonly ApplicationDbContext _context;
        private readonly GoogleMapsService _googleMapsService;

        public TripService(ApplicationDbContext context , GoogleMapsService googleMapsService)
        {
            _context = context;
            _googleMapsService = googleMapsService;
        }
        public void StartTrip(int userId , string startLocation, string endLocation)
        {
            if (string.IsNullOrWhiteSpace(startLocation) || string.IsNullOrWhiteSpace(endLocation))
                throw new ArgumentException("Start and End locations cannot be empty.");

            var distance = _googleMapsService.GetDistanceBetweenPlacesByName(startLocation, endLocation).Result;

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user is null)
                throw new Exception("User not found");

            var trip = new Trip
            {
                UserId = userId,
                StartLocation = startLocation.Trim(),
                EndLocation = endLocation.Trim(),
                Distance = distance,
                StartTime = DateTime.UtcNow,
                Status = TripStatus.InProgress
            };

            user.TripCount += 1;
            _context.Trips.Add(trip);
            _context.SaveChanges();
        }

        public bool AbortTrip(int tripId)
        {
            var trip = _context.Trips.FirstOrDefault(t => t.Id == tripId);
            if (trip is null)
                throw new KeyNotFoundException("Trip not found.");

            if (trip.Status is TripStatus.Cancelled or TripStatus.Completed)
                throw new InvalidOperationException("Cannot abort this trip.");

            trip.EndTime = DateTime.UtcNow;
            trip.Status = TripStatus.Cancelled;
            
            return _context.SaveChanges() > 0;
        }

        public bool EndTrip(int tripId, int? feedback = null)
        {
            var trip = _context.Trips.FirstOrDefault(t => t.Id == tripId);
            if (trip is null)
                throw new KeyNotFoundException("Trip not found.");

            if (trip.Status == TripStatus.Cancelled || trip.Status == TripStatus.Completed)
                throw new InvalidOperationException("Cannot end this trip.");

            trip.EndTime = DateTime.UtcNow;
            trip.Status = TripStatus.Completed;

            
            if (feedback is >= 1 and <= 5)
                trip.FeedbackRate = feedback.Value;
            else if (feedback.HasValue)
                throw new ArgumentOutOfRangeException(nameof(feedback), "Feedback must be between 1 and 5.");

            return _context.SaveChanges() > 0;
        }

        public bool DeleteTrip(int tripId)
        {
            var trip = _context.Trips.FirstOrDefault(t => t.Id == tripId);
            if (trip is null)
                throw new KeyNotFoundException("Trip not found.");

            if (trip.Status != TripStatus.Cancelled)
                throw new InvalidOperationException("Only cancelled trips can be deleted.");

            _context.Trips.Remove(trip);
            return _context.SaveChanges() > 0;
        }
    }
}
