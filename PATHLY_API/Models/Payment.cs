using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{
	public class Payment
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int PaymentId { get; set; }

		[Required]
		[ForeignKey("User")]
		public int UserId { get; set; }

		[Required]
		[ForeignKey("SubscriptionPlan")]
		public int SubscriptionPlanId { get; set; }

		[Required, Column(TypeName = "decimal(18, 2)")]
		public decimal Amount { get; set; }

		[Required]
		public DateTime PaymentDate { get; set; }

		[Required]
		public DateTime ReportTime { get; set; }

		[Required]
		[StringLength(50)] // Max length
		public string PaymentMethod { get; set; }

		[Required]
		[StringLength(20)] // Max length
		public string PaymentStatus { get; set; } // e.g., "Pending", "Completed", "Refunded"

		[StringLength(50)]
		public string? TransactionId { get; set; } // Store PayPal transaction ID


		public User User { get; set; }
		public SubscriptionPlan SubscriptionPlan { get; set; }

		public string GetPaymentDetails()
		{
			return $"Payment ID: {PaymentId}, Amount: {Amount}, Status: {PaymentStatus}, Method: {PaymentMethod}, Date: {PaymentDate}";
		}

		public bool IsPaymentSuccessful()
		{
			return PaymentStatus == "Completed";
		}
	}
}
