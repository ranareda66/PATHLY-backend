namespace PATHLY_API.Models
{
    public class TripRoad
    {
        public int RoadId { get; set; }
        public int TripId { get; set; }

        public Road Road { get; set; }
        public Trip Trip{ get; set; }
    }
}
