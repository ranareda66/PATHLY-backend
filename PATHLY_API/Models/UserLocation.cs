using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
	public class UserLocation
	{
		[Key] // Primary Key
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
		public int UserLocationId { get; set; } // Primary Key

		[ForeignKey("User")]
		public int UserId { get; set; } // Foreign Key

		[Required]
		[Column(TypeName = "decimal(9, 6)")] // Precision for latitude
		public decimal Latitude { get; set; }

		[Required]
		[Column(TypeName = "decimal(9, 6)")] // Precision for latitude
		public decimal Longitude { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }
		public virtual User User { get; set; }

		public string GetLocationInfo()
		{
			return $"Latitude: {Latitude}, Longitude: {Longitude}, Timestamp: {Timestamp}";
		}
	}
}
