using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Dto;

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

		[HttpPost("process-payment")]
		public async Task<ActionResult<PaymentResponse>> ProcessPayment([FromBody] PaymentRequest request)
		{
			try
			{
				var response = await _paymentService.ProcessPayPalPaymentAsync(request.UserId, request.SubscriptionPlanId);
				return Ok(response);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(ex.Message);
			}
		}

		[HttpGet("success")]
		public async Task<IActionResult> Success([FromQuery] string orderId)
		{
			bool success = await _paymentService.CompletePayPalPaymentAsync(orderId);
			if (success)
			{
				return Redirect("yourapp://payment-success"); // Redirect to Flutter app
			}
			else
			{
				return Redirect("yourapp://payment-failed"); // Redirect to Flutter app
			}
		}

		[HttpGet("cancel")]
		public IActionResult Cancel()
		{
			return Redirect("yourapp://payment-cancelled"); // Redirect to Flutter app
		}
	}
}
