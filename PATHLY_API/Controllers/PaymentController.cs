using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PATHLY_API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentController : ControllerBase
	{
		private readonly PaymentService _paymentService;
		public PaymentController(PaymentService paymentService) => _paymentService = paymentService;


        [HttpPost("create")]
        public async Task<IActionResult> CreateSubscription([FromForm] int subscriptionPlanId)
        {
            try
            {
                var user = HttpContext.User; 

                if (user == null || !user.Identity.IsAuthenticated)
                    return Unauthorized("User is not authenticated");

				await _paymentService.CreateSubscriptionAsync(user, subscriptionPlanId);

				return Ok(new {Message = "User Stored In DataBase successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
			catch (DbUpdateException dbEx)
			{
				return StatusCode(500, $"Database Error: {dbEx.InnerException?.Message ?? dbEx.Message}");
			}
			catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
	}
}
