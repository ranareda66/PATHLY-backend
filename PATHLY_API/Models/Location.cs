using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PATHLY_API.Models
{
    public class Location
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; }

        [Required, Column(TypeName = "decimal(9, 6)")]
        public decimal Latitude { get; set; }

        [Required, Column(TypeName = "decimal(9, 6)")]
        public decimal Longitude { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public virtual User User { get; set; }
    }
}
