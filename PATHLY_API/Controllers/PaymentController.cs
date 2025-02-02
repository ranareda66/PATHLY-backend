using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PATHLY_API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentController : ControllerBase
	{
		private readonly PaymentService _paymentService;

		public PaymentController(PaymentService paymentService)
		{
			_paymentService = paymentService;
		}

		[HttpPost("paypal/create")]
		public async Task<IActionResult> CreatePayPalPayment([FromBody] PaymentRequest request)
		{
			string orderId = await _paymentService.ProcessPayPalPaymentAsync(request.UserId, request.SubscriptionPlanId);
			return Ok(new { orderId });
		}

		[HttpPost("paypal/capture")]
		public async Task<IActionResult> CapturePayPalPayment([FromBody] CapturePaymentRequest request)
		{
			bool success = await _paymentService.CompletePayPalPaymentAsync(request.PaymentId, request.OrderId);
			return success ? Ok("Payment completed!") : BadRequest("Payment failed.");
		}
	}

	public class PaymentRequest
	{
		public int UserId { get; set; }
		public int SubscriptionPlanId { get; set; }
	}

	public class CapturePaymentRequest
	{
		public int PaymentId { get; set; }
		public string OrderId { get; set; }
	}
}

