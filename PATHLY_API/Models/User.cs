using Microsoft.AspNetCore.Identity;
using PATHLY_API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PATHLY_API.Models
{
    public class User : IdentityUser<int>
    {
        public DateTime CreatedAt { get; internal set; }

        public bool IsActive { get; internal set; }

        public int TripCount { get; set; } = 0;

        public ICollection<Trip> Trips { get; set; }
        public virtual List<Payment> Payments { get; set; }
        public ICollection<Search> Searchs { get; set; }
        public ICollection<Report> Reports { get; set; }
        public List<RefreshToken>? RefreshTokens { get; set; }
        public List<UserSubscription> UserSubscriptions { get; set; }
        public SubscriptionStatus SubscriptionStatus { get; set; }
    }
}