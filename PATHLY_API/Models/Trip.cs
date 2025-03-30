using PATHLY_API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
    public class Trip
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string StartLocation { get; set; }
        [Required]
        public string EndLocation { get; set; }

        [Required]
        public decimal Distance { get; set; }

        public DateTime StartTime { get; internal set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }

        [Required]
        public TripStatus Status { get; set; }

        [Range(1, 5)]
        public int? FeedbackRate { get; set; } = null;


        [Required, ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<TripRoad> TripRoads { get; set; } = new List<TripRoad>(); 
    }
}
