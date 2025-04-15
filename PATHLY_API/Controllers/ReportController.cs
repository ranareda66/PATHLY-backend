using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;
using PATHLY_API.Services;

namespace PATHLY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ReportService _reportService;
        public ReportController(ReportService reportService) => _reportService = reportService;

        // Create Report for Users ✅
        [HttpPost("Create")]
        public async Task<IActionResult> CreateReport([FromForm] ReportRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdReport = await _reportService.CreateReportAsync(request, User);
            return CreatedAtAction(nameof(GetReportById), new { reportId = createdReport.Id }, createdReport);
        }

        // Return a specific report using ID ✅
        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetReportById(int reportId)
        {
            var report = await _reportService.GetReportByIdAsync(reportId);
            return Ok(report);
        }

    }
}
