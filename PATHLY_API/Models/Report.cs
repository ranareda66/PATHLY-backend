using PATHLY_API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PATHLY_API.Models
{
    public class Report
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportType ReportType { get; set; }

        [Required, MaxLength(800)]
        public string Description { get; set; }

        [Required, JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportStatus Status { get; set; }

        public DateTime CreatedAt { get; internal set; } = DateTime.UtcNow;

        [Required, ForeignKey("User")]
        public int UserId { get; set; }


        [ForeignKey("Location") , JsonIgnore]
        public int LocationId { get; set; }

        public Location Location { get; set; }

        public Image Image { get; set; }

        [JsonIgnore]
        public User User { get; set; }

    }

}
