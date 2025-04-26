using PATHLY_API.Models.Enums;

namespace PATHLY_API.Dto
{
    public class TripResponseDto
    {
        public int Id { get; set; }
        public double StartLatitude { get; set; }
        public double StartLongitude { get; set; }
        public double EndLatitude { get; set; }
        public double EndLongitude { get; set; }
        public double Distance { get; set; }
        public string RoutePolyline { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TripStatus Status { get; set; }
        public int? FeedbackRate { get; set; }
        public bool UsedRoadPrediction { get; set; }
        public RouteDto Route { get; set; }
    }
}
