using Microsoft.AspNetCore.Identity;
using PATHLY_API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace PATHLY_API.Models
{
    public class User : IdentityUser<int>
    {
        public User()
        {
            CreatedAt = DateTime.UtcNow; 
            IsActive = true;  
            Trips = new List<Trip>();
            Payments = new List<Payment>();
            Searchs = new List<Search>();
            Reports = new List<Report>();
            RefreshTokens = new List<RefreshToken>();
            UserSubscriptions = new List<UserSubscription>();
        }

        public DateTime CreatedAt { get; internal set; }

        public bool IsActive { get; internal set; }

        public int TripCount { get; set; } = 0;

        [JsonIgnore]
        public virtual Location Location { get; set; }

        public virtual ICollection<Trip> Trips { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<Search> Searchs { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<RefreshToken>? RefreshTokens { get; set; }
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; }

        public SubscriptionStatus SubscriptionStatus { get; set; }
    }
}