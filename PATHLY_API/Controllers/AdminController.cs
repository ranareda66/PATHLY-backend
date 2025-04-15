using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Models.Enums;
using PATHLY_API.Services;

namespace PATHLY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        public AdminController(AdminService adminService) => _adminService = adminService;

        // Get All Reports Related to user ✅
        [HttpGet("reports/user")]
        public async Task<IActionResult> GetReports([FromQuery] int userId)
        {
            var reports = await _adminService.GetUserReportsAsync(userId);
            return Ok(reports);
        }

        // Return Reports based on report status ✅
        [HttpGet("reports/status")]
        public async Task<IActionResult> GetReportsByStatus([FromQuery] ReportStatus? status)
        {
            var reports = await _adminService.GetReportsByStatusAsync(status);
            return Ok(reports);
        }

        // Update Report Status ✅
        [HttpPut("reports/{reportId}/newstatus")]
        public async Task<IActionResult> UpdateReportStatus(int reportId, [FromBody] ReportStatus newStatus)
        {
            var success = await _adminService.UpdateReportStatusAsync(reportId, newStatus);
            return success ? Ok("Report Status Updated successfully") : NotFound();
        }
    }
}
