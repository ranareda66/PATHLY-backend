using PATHLY_API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
    public class Trip
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public double StartLatitude { get; set; }

        [Required]
        public double StartLongitude { get; set; }

        [Required]
        public double EndLatitude { get; set; }

        [Required]
        public double EndLongitude { get; set; }

        [Required]
        public double Distance { get; set; }

        public string RoutePolyline { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }

        [Required]
        public TripStatus Status { get; set; }

        [Range(1, 5)]
        public int? FeedbackRate { get; set; }

        public bool UsedRoadPrediction { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<TripRoad> TripRoads { get; set; } = new List<TripRoad>();
    }


}