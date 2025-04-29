using PATHLY_API.Dto;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;

namespace PATHLY_API.Services
{
    public interface ITripService
    {
        Task<TripResultDto> StartTripWithCoordinatesAsync(
            double startLat, double startLng,
            double endLat, double endLng,
            bool requestRoadPrediction);

        Task<List<HazardNotificationDto>> GetUpcomingHazards(int tripId, double currentLat, double currentLng);
        Task<bool> HasActiveSubscription(int userId);
        Task<bool> EndTripAsync(int tripId, int? feedback = null);
        Task<bool> AbortTripAsync(int tripId);
        Task<bool> DeleteTripAsync(int tripId);
        Task<List<TripDto>> GetUserTripsAsync(int userId, DateTime? startDate = null, TripStatus? status = null);
        Task<TripResponseDto> GetTripDetailsAsync(int tripId);
    }
}