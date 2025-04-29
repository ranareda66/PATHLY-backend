using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PATHLY_API.Models.Enums;

namespace PATHLY_API.Models
{
    public class Trip
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,15)")]
        public decimal StartLatitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,15)")]
        public decimal StartLongitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,15)")]
        public decimal EndLatitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,15)")]
        public decimal EndLongitude { get; set; }

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