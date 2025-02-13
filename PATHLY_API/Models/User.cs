using PATHLY_API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PATHLY_API.Models
{
	public class User
	{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } 

		[Required , StringLength(15, MinimumLength = 3)]
		public string Name { get; set; }

        [Required , EmailAddress]
		public string Email { get; set; }

		[Required, StringLength(20)]
		public string Password { get; set; }
        public DateTime CreatedAt { get; set; }

        [Required, StringLength(15)]
		public SubscriptionStatus SubscriptionStatus { get; set; }

		[ForeignKey("UserLocations")]
		public int UserLocationId { get; set; }

        [Required]
        public bool IsActive { get; internal set; }

        [Required]
		public int TripCount { get; set; } = 0; 

		public int MaxFreeTrips { get; set; } = 10;
        public List<Trip> Trips { get; set; }
		public virtual List<Payment> Payments { get; set; }
        public ICollection<Search> Searchs { get; set; }
		public ICollection<Report> Reports { get; set; } 

        public UserSubscription UserSubscription { get; set; }
        public Location Location { get; set; }
    }
}

