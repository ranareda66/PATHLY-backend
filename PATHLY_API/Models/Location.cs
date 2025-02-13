using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
	public class Location
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } 

		[Required , Column(TypeName = "decimal(9, 6)")] 
		public decimal Latitude { get; set; }

        [Required, Column(TypeName = "decimal(9, 6)")] 
		public decimal Longitude { get; set; }
        [Required]
        public DateTime UpdatedAt { get; internal set; } = DateTime.UtcNow;

        [Required , ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

    }
}
