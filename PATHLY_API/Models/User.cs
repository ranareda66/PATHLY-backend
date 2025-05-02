using Microsoft.AspNetCore.Identity;
using PATHLY_API.Models.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
    public class User : IdentityUser<int>
    {
        public User()
        {
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            Trips = new List<Trip>();
            Searchs = new List<Search>();
            Reports = new List<Report>();
            RefreshTokens = new List<RefreshToken>();
            UserSubscriptions = new List<UserSubscription>();
            FreeTripsUsed = 0;
        }

        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int TripCount { get; set; }
        public int FreeTripsUsed { get; set; }
        public SubscriptionStatus SubscriptionStatus { get; set; }

        [ForeignKey("Location")]
        public int? LocationId { get; set; }

        public virtual ICollection<Trip> Trips { get; set; }
        public virtual ICollection<Search> Searchs { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; }
    }
}