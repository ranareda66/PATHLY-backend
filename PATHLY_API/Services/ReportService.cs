using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Dto;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace PATHLY_API.Services
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public ReportService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        // Create Report from user ✅
        public async Task<Report> CreateReportAsync(ReportRequestModel request, ClaimsPrincipal user)
        {

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;


            if (!int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException($"Invalid User ID format. Value received: '{userIdClaim}'");

            var report = new Report
            {
                ReportType = request.ReportType.Value,
                Description = request.Description.Trim(),
                Latitude = request.Latitude.Value,
                Longitude = request.Longitude.Value,
                UserId = userId
            };


            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();

            if (request.Image is not null && request.Image.Length > 0)
                await UploadImageAsync(report.Id, request.Image);

            return report;
        }

        // Search for Report By ID ✅
        public async Task<Report> GetReportByIdAsync(int reportId)
        {
            if (reportId <= 0)
                throw new ArgumentException("Invalid Report ID.");

            var report = await _context.Reports
                .Include(r => r.Image)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report is null)
                throw new KeyNotFoundException($"Report with ID {reportId} not found.");

            return new Report
            {
                Id = report.Id,
                Description = report.Description,
                ReportType = report.ReportType,
                CreatedAt = report.CreatedAt,
                Status = report.Status,
                UserId = report.UserId,
                Latitude = report.Latitude,
                Longitude = report.Longitude,
                Image = report.Image
            };
        }

        private async Task<Image> UploadImageAsync(int reportId, IFormFile file)
        {

            if (file?.Length <= 0)
                throw new ArgumentException("File is not valid");

            string fileExtension = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("Invalid file type. Only JPG and PNG are allowed.");

            long maxFileSize = 5 * 1024 * 1024; 
            if (file.Length > maxFileSize)
                throw new ArgumentException("File size exceeds the allowed limit of 5MB.");

            if (!await _context.Reports.AnyAsync(r => r.Id == reportId))
                throw new KeyNotFoundException($"Report with ID {reportId} not found.");

            string webRootPath = _environment.WebRootPath ?? throw new InvalidOperationException("Web root path is not configured.");

            string uploadsFolder = Path.Combine(webRootPath, "Uploads");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{DateTime.UtcNow.Ticks}_{Guid.NewGuid()}{fileExtension}";
            string imagePath = Path.Combine(uploadsFolder, uniqueFileName);
            string relativePath = Path.Combine("Uploads", uniqueFileName).Replace("\\", "/"); 


            using (var stream = new FileStream(imagePath, FileMode.Create))
                await file.CopyToAsync(stream);


            var image = new Image
            {
                ReportId = reportId,
                ImagePath = relativePath,
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            return image;
        }
       
    }
}
