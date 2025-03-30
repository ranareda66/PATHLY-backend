using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using PATHLY_API.Models.Enums;
using PayPalCheckoutSdk.Orders;

namespace PATHLY_API.Models
{
	public class Payment
	{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required , Column(TypeName = "decimal(18, 2)")]
		public decimal Amount { get; set; }

		public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

		[Required] 
		public PaymentMethodType PaymentMethod { get; set; }

		[Required]
		public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public string TransactionId { get; set; } = string.Empty; 

        [Required , ForeignKey("SubscriptionPlan")]
		public int SubscriptionPlanId { get; set; }

		[Required , ForeignKey("User")]
		public int UserId { get; set; }
        public virtual User User { get; set; } 
        public virtual SubscriptionPlan SubscriptionPlan { get; set; }

    }
}
