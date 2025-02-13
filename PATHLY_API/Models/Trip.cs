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

        [Required]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime EndTime { get; set; }

        [Required, StringLength(15)]
        public TripStatus Status { get; set; }

        public int? FeedbackRate { get; set; } = 0;

        [Required, ForeignKey("User")]
        public int UserId { get; set; }

        public User User { get; set; }

        public ICollection<TripRoad> TripRoads { get; set; }
    }
}
