using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
    public class RoadRecommendation
    {
        [Key]
        public int RecommendationId { get; set; }

        [Required]
        public int UserId { get; set; } // Foreign key reference to User

        [Required]
        public int RoadId { get; set; } // Foreign key reference to Road

        public decimal RQIScore { get; set; }
        public bool ShortestPath { get; set; }
        public bool MostUsed { get; set; }
        public bool Recommended { get; set; } = false;

        // Navigation properties
        [ForeignKey("RoadId")]
        public Road? Road { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
