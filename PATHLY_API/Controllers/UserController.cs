using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PATHLY_API.Models;
using PATHLY_API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PATHLY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly ReportService _reportService; 

        private readonly UserService _userService;

        public UserController(UserService userService , ReportService reportService)
        {
            _userService = userService;
            _reportService = reportService;
        }

        [HttpPost("change-email")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "Invalid data.", Errors = ModelState.Values.SelectMany(v => v.Errors) });

            var result = await _userService.ChangeEmailAsync(model.UserId, model.NewEmail, model.Password);

            if (result == "User not found.")
                return NotFound(result);

            if (result == "Invalid password." || result == "Email is already in use." || result.StartsWith("Failed to change email."))
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "Invalid data.", Errors = ModelState.Values.SelectMany(v => v.Errors) });

            var result = await _userService.ChangePasswordAsync(model.UserId, model.CurrentPassword, model.NewPassword);

            if (result == "User not found." || result == "Current password is incorrect." || result == "Failed to change password.")
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("create-report")]
        public async Task<IActionResult> CreateReport([FromForm] string reportType,[FromForm] string description,[FromForm] IFormFile file,[FromForm] decimal? latitude,[FromForm] decimal? longitude)
        {

            if (!Request.Cookies.TryGetValue("UserId", out string userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized("User is not authenticated.");


            var report = await _reportService.CreateReportAsync(reportType, description, userId, file, latitude, longitude);
            return CreatedAtAction(nameof(GetUserReports), new { userId }, report);
        }

        [HttpGet("reports/user/{userId}")]
        public async Task<IActionResult> GetUserReports(int userId)
        {
            var reports = await _userService.GetUserReportsAsync(userId);

            if (userId <= 0)
                return BadRequest("Invalid user ID.");

            if (reports.Count == 0)
                return BadRequest( $"No reports found for user with ID {userId}.");
            return Ok(reports);
        }
    }
}
