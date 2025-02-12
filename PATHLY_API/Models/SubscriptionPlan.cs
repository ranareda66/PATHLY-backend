using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{
	public class SubscriptionPlan
	{
		[Key] 
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
		public int Id { get; set; }

		[Required , StringLength(100)] 
		public string PlanName { get; set; }

		[Required , Column(TypeName = "decimal(18, 2)")]
		public decimal Price { get; set; }

		[Required , StringLength(255)]
		public string Description { get; set; }

		[Required]
		public int DurationInMonths { get; set; }
		
		public Payment Payments { get; set; }

        public List<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();


    }
}
