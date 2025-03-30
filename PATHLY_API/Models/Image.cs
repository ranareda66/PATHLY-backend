using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PATHLY_API.Models
{
    public class Image
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string ImagePath { get; set; }

        [ForeignKey("Report")]
        public int ReportId { get; set; }

        [JsonIgnore]
        public Report Report { get; set; }
    }
}
