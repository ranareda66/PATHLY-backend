using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
    public class TripRoad
    {
        [Required, ForeignKey("Road")]
        public int RoadId { get; set; }

        [Required, ForeignKey("Trip")]
        public int TripId { get; set; }

        public virtual Road Road { get; set; }  
        public virtual Trip Trip { get; set; }  
    }
}
