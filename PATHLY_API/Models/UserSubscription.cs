using Microsoft.EntityFrameworkCore;
using PATHLY_API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
    public class UserSubscription
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public SubscriptionStatus Status { get; set; }


        [Required, ForeignKey("User")]
        public int UserId { get; set; }


        [Required, ForeignKey("SubscriptionPlan")]
        public int SubscriptionPlanId { get; set; }

        public virtual User User { get; set; }
        public virtual SubscriptionPlan SubscriptionPlan { get; set; }
    }
}