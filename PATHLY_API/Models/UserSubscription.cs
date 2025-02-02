using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
	public class UserSubscription
	{
		[Key] 
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
		public int Id { get; set; } 

		[Required]
		[ForeignKey("User")]
		public int UserId { get; set; } 

		[Required]
		[ForeignKey("SubscriptionPlan")]
		public int SubscriptionPlanId { get; set; } 

		[Required] 
		public DateTime StartDate { get; set; }

		[Required] 
		public DateTime EndDate { get; set; }

		public User User { get; set; }
		public SubscriptionPlan SubscriptionPlan { get; set; }

	}
}
