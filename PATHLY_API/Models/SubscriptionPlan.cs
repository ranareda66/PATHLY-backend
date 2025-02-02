using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{
	public class SubscriptionPlan
	{
		[Key] 
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
		public int SubscriptionPlanId { get; set; }

		[Required] 
		[StringLength(100)] 
		public string PlanName { get; set; }

		[Required]
		[Column(TypeName = "decimal(18, 2)")] 
		public decimal Price { get; set; }

		[Required]
		[StringLength(255)] 
		public string Description { get; set; }

		[Required]
		public bool Recurring { get; set; }

		[Required]
		public int DurationInMonths { get; set; } // Duration in months

		
		public virtual List<Payment> Payments { get; set; }

		public string GetPlanInfo()
		{
			return $"Plan Name: {PlanName}, Price: {Price}, Description: {Description}, Recurring: {Recurring}, Duration: {DurationInMonths} months";
		}

		public bool IsRecurring()
		{
			return Recurring;
		}
	}
}
