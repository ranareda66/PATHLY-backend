using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using PATHLY_API.Models.Enums;

namespace PATHLY_API.Models
{
	public class Payment
	{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

		[Required, Column(TypeName = "decimal(18, 2)")]
		public decimal Amount { get; set; }

		[Required]
		public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

		[Required , StringLength(50)] // Max length
		public string PaymentMethod { get; set; }

		[Required , StringLength(20)] // Max length
		public PaymentStatus PaymentStatus { get; set; }

		[StringLength(50)]
		public string? TransactionId { get; set; } // Store PayPal transaction ID


		[Required , ForeignKey("SubscriptionPlan")]
		public int SubscriptionPlanId { get; set; }

		[Required , ForeignKey("User")]
		public int UserId { get; set; }
		public SubscriptionPlan SubscriptionPlan { get; set; }
		public User User { get; set; }

    }
}
