using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Dto;

namespace PATHLY_API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentController : ControllerBase
	{
		private readonly PaymentService _paymentService;
		public PaymentController(PaymentService paymentService) => _paymentService = paymentService;


        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromForm] int subscriptionPlanId)
        {
            try
            {
                var user = HttpContext.User; 

                if (user == null || !user.Identity.IsAuthenticated)
                    return Unauthorized("User is not authenticated");

                var orderId = await _paymentService.CreatePaymentAsync(user, subscriptionPlanId);

                return Ok(new { OrderId = orderId, Message = "Payment order created successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

		[HttpPost("capture")]
		public async Task<IActionResult> CapturePayment([FromForm] string orderId)
		{
			try
			{
				bool success = await _paymentService.CapturePaymentAsync(orderId);
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

		[HttpPost("cancel")]
		public async Task<IActionResult> CancelPayment([FromForm] string orderId)
		{
			try
			{
				bool success = await _paymentService.CancelPaymentAsync(orderId);
				return Ok("Payment Canceled successfully.");
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
