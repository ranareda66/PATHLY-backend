using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{
    public class SearchHistory
    {
        [Key]
        public int SearchId { get; set; }

        [Required]
        public int UserId { get; set; } // Foreign key (assuming User table exists)

        [Required]
        [MaxLength(500)] // Limit the length of search queries
        public string SearchQuery { get; set; } // The search query entered by the user

        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Range(-180, 180)]
        public decimal Longitude { get; set; }

        public User User { get; set; }
    }
}
