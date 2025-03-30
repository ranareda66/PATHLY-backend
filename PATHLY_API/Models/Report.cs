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
        public ProblemType ReportType { get; set; }

        [Required, MaxLength(800)]
        public string Description { get; set; }

        [Required, JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public DateTime CreatedAt { get; internal set; } = DateTime.UtcNow;

        [Required, Column(TypeName = "decimal(9, 6)")]
        public decimal Latitude { get; set; }

        [Required, Column(TypeName = "decimal(9, 6)")]
        public decimal Longitude { get; set; }


        [ForeignKey("User")]
        public int UserId { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }
        public virtual Image Image { get; set; }

    }

}
