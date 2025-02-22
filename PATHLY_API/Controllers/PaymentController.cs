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

		[HttpPost("create-order")]
		public async Task<ActionResult<PaymentResponse>> CreateOrder([FromBody] PaymentRequest request)
		{
			try
			{
				var response = await _paymentService.CreatePaymentAsync(request.UserId, request.SubscriptionPlanId);
				return Ok(response);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(ex.Message);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { Message = "An error occurred: " + ex.Message });
			}
		}

		[HttpPost("capture-payment")]
		public async Task<IActionResult> CapturePayment([FromBody] CompletePaymentRequest request)
		{
			try
			{
				bool success = await _paymentService.CapturePaymentAsync(request.OrderId);
				return Ok(new { Success = success });
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(ex.Message);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { Message = "An error occurred: " + ex.Message });
			}
		}

		[HttpPost("cancel-payment")]
		public async Task<IActionResult> CancelPayment([FromBody] CompletePaymentRequest request)
		{
			try
			{
				bool success = await _paymentService.CancelPaymentAsync(request.OrderId);
				return Ok(new { Success = success });
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(ex.Message);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { Message = "An error occurred: " + ex.Message });
			}
		}
	}
}
