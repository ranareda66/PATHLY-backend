using PATHLY_API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
    public class Report
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string ReportType { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        [Required, StringLength(15)] 
        public ReportStatus Status { get; set; }

        [Required, ForeignKey("User")]
        public int UserId { get; set; }

        [Required, ForeignKey("Road")]
        public int RoadId { get; set; }

        [Required, ForeignKey("UserLocation")]
        public int UserLocationId { get; set; }

        public ICollection<Image> Attachments = new List<Image>();

        public User User { get; set; }

    }

}
