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

        // Return a specific report using ID
        [HttpGet("reports/{reportId}")]
        public async Task<IActionResult> GetReportById(int reportId)
        {
            var report = await _adminService.GetReportByIdAsync(reportId);
            return Ok(report);
        }

        // Return Reports based on report status ✅
        [HttpGet("reports/status/{status}")]
        public async Task<IActionResult> GetReportsByStatus(ReportStatus status)
        {
            var reports = await _adminService.GetReportsByStatusAsync(status);
            return Ok(reports);
        }

        // Update Report Status
        [HttpPut("reports/status/{reportId}")]
        public async Task<IActionResult> UpdateReportStatus(int reportId, [FromBody] ReportStatus newStatus)
        {
            var success = await _adminService.UpdateReportStatusAsync(reportId, newStatus);
            return success ? Ok() : NotFound();
        }
    }

}
