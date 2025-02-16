using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{
    public class TripRoad
    {
        [Required]
        public int RoadId { get; set; }
        [Required]
        public int TripId { get; set; }

        public Road Road { get; set; }
        public Trip Trip{ get; set; }
    }
}
