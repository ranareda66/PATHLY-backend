using PATHLY_API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
    public class Report
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string ReportType { get; set; }

        [Required, MaxLength(800)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; internal set; } = DateTime.UtcNow;


        [Required] 
        public ReportStatus Status { get; set; }

        [Required, ForeignKey("User")]
        public int UserId { get; set; }


        [Required, ForeignKey("Location")]
        public int LocationId { get; set; }

        public Image Image { get; set; }

        public User User { get; set; }

    }

}
