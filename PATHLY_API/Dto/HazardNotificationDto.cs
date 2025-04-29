namespace PATHLY_API.Dto
{
    public class HazardNotificationDto
    {
        public string HazardType { get; set; } // "pump", "hole", "bad_road"
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double DistanceAhead { get; set; } // in meters
        public string WarningMessage { get; set; }
    }
}
