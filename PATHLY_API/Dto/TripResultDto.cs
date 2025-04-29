using PATHLY_API.Models;

namespace PATHLY_API.Dto
{
    public class TripResultDto
    {
        public TripDto Trip { get; set; }
        public RouteDto Route { get; set; }
        public List<ClusterGroup> RoadPrediction { get; set; }
        public List<HazardNotificationDto> HazardNotifications { get; set; }
        public int? FreeTripsRemaining { get; set; }
        public bool RequiresSubscription { get; set; }
    }
}
