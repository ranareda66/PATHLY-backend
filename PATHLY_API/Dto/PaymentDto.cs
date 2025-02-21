using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Dto
{
	public class PaymentRequest
	{
		[Required]
		public int UserId { get; set; }

		[Required]
		public int SubscriptionPlanId { get; set; }
	}

	//public class CompletePaymentRequest
	//{
	//	public string OrderId { get; set; }
	//}

	public class PaymentResponse
	{
		public string OrderId { get; set; }
		public string ApprovalUrl { get; set; }
	}
}
