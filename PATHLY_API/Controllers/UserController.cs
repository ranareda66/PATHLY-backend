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

            return await _userService.ChangeEmailAsync(User, model.NewEmail, model.Password);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "Invalid data.", Errors = ModelState.Values.SelectMany(v => v.Errors) });

            return await _userService.ChangePasswordAsync(User, model.CurrentPassword, model.NewPassword);
        }

        [HttpPost("create-report")]
        public async Task<IActionResult> CreateReport([FromForm] string reportType, [FromForm] string description, [FromForm] IFormFile image, [FromForm] decimal latitude, [FromForm] decimal longitude)
        {
            var report = await _reportService.CreateReportAsync(reportType, description, User, image, latitude, longitude);
            return CreatedAtAction(nameof(GetReports), new { userId = report.UserId }, report);
        }
       
        [HttpGet("reports/userId/{userId}")] 
        public async Task<IActionResult> GetReports(int userId)
        {
            var reports = await _reportService.GetUserReportsAsync(userId);
            return Ok(reports);
        }
    }
}
