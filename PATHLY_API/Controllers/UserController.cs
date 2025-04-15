using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Models;
using PATHLY_API.Services;

namespace PATHLY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService) => _userService = userService;

        // Change Emaill for Users ✅
        [HttpPost("change-email")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "Invalid data.", Errors = ModelState.Values.SelectMany(v => v.Errors) });

            var result = await _userService.ChangeEmailAsync(User, model.NewEmail, model.Password);

            return result switch
            {
                "User ID not found." => BadRequest(result),
                "User not found." => NotFound(result),
                "Invalid password." => Unauthorized(result),
                "New email cannot be the same as the current email." => BadRequest(result),
                "Email is already in use." => Conflict(result),
                _ when result.StartsWith("Failed to change email.") => BadRequest(result),
                "Email changed successfully." => Ok(result),
                _ => StatusCode(500, "An unexpected error occurred.")
            };
        }

        // Change Password for Users ✅
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "Invalid data.", Errors = ModelState.Values.SelectMany(v => v.Errors) });

            var result = await _userService.ChangePasswordAsync(User, model.Password, model.NewPassword);

            return result switch
            {
                "User ID not found." => BadRequest(result),
                "User not found." => NotFound(result),
                "Current password is incorrect." => Unauthorized(result),
                "New password cannot be the same as the current password." => BadRequest(result),
                _ when result.StartsWith("Failed to change password.") => BadRequest(result),
                "Password changed successfully." => Ok(result),
                _ => StatusCode(500, "An unexpected error occurred.")
            };
        }


        [HttpPost("subscribe-to-plan")]
        public async Task<IActionResult> SubscribeToPlan([FromQuery] int SubscriptionPlanId)
        {
            if (SubscriptionPlanId <= 0)
                return BadRequest("Invalid subscription request.");

            try
            {
                var user = HttpContext.User;
                await _userService.SubscribeToPlanAsync(User, SubscriptionPlanId);
                return Ok("Subscription is done successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("subscription-status")]
        public async Task<IActionResult> GetUserSubscriptionStatus([FromQuery] int userId)
        {
            var status = await _userService.GetUserSubscriptionStatusAsync(userId);
            return Ok(new { subscriptions = status });
        }
    }
}
