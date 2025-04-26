using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PATHLY_API.Models
{
    public class Location
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; }
        public string Address { get; set; }

        public DateTime UpdatedAt { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }
    }
}
