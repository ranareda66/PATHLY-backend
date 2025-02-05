using PATHLY_API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PATHLY_API.Models
{
	public class User
	{
		[Key] // Primary Key
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
		public int UserId { get; set; } // Primary Key

		[Required]
		[StringLength(15, MinimumLength = 3)]
		public string Name { get; set; }

		[Required]
		[EmailAddress] // Validate email format
		public string Email { get; set; }

		[Required, StringLength(20)]
		public string PasswordHash { get; set; }

		[Required, StringLength(15)]
		public UserRole Role { get; set; }

		[Required, StringLength(15)]
		public SubscriptionStatus SubscriptionStatus { get; set; }

		[Required]
		public DateTime SubscriptionStartDate { get; set; }

		[Required]
		public DateTime? SubscriptionEndDate { get; set; }

		[ForeignKey("UserLocations")]
		public int UserLocationId { get; set; } // Foreign Key

		[Required]
		public int TripCount { get; set; } = 0; // Default value

		[Required]
		public int MaxFreeTrips { get; set; } = 3;
		public virtual UserLocation UserLocations { get; set; }
		public virtual List<Payment> Payments { get; set; }
		public virtual List<UserPreferences> UserPreferences { get; set; }


        public ICollection<SearchHistory> SearchHistories { get; set; }
        public ICollection<RoadRecommendation> RoadRecommendations { get; set; }
        public ICollection<UserFeedback>? UserFeedbacks { get; set; } = new List<UserFeedback>();

        
		public bool CheckSubscriptionValidity()
		{
			return DateTime.Now <= SubscriptionEndDate;
		}

		public bool IsSubscribed()
		{
			return SubscriptionStatus == SubscriptionStatus.Active;
		}
	} 
}

