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
        private readonly UserManager<User> _userManager;
        public ReportService(ApplicationDbContext context, IWebHostEnvironment environment, UserManager<User> userManager)
        {
            _context = context;
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _userManager = userManager;
        }

        public async Task<Report> CreateReportAsync(string reportType, string reportDescription, ClaimsPrincipal user, IFormFile image, decimal latitude, decimal longitude)
        {
            if (!Enum.TryParse(typeof(ReportType), reportType, true, out var validReportType))
                throw new ArgumentException($"Invalid report type. Allowed values: {string.Join(", ", Enum.GetNames(typeof(ReportType)))}");


            if (string.IsNullOrWhiteSpace(reportDescription))
                throw new ArgumentException("Report description cannot be empty.", nameof(reportDescription));

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;


            if (!int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException($"Invalid User ID format. Value received: '{userIdClaim}'");



            Location location = null;

            if (latitude != 0 && longitude != 0)
            {
                int locationId = await GetLocationIdByCoordinatesAsync(latitude, longitude);

                if (locationId == 0)
                {
                    // Create New Location if not Exists
                    location = new Location
                    {
                        Latitude = latitude,
                        Longitude = longitude,
                        UserId = userId
                    };

                    _context.Locations.Add(location);
                    await _context.SaveChangesAsync();
                }
                else
                    location = await _context.Locations.FindAsync(locationId);
            }

            var report = new Report
            {
                ReportType = (ReportType)validReportType,
                Description = reportDescription.Trim(),
                Status = ReportStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Location = location,
                UserId = userId
            };

            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();

            if (image is not null && image.Length > 0)
                await UploadImageAsync(report.Id, image);

            return report;
        }

        private async Task<int> GetLocationIdByCoordinatesAsync(decimal latitude, decimal longitude)
        {
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => Math.Abs(l.Latitude - latitude) < 0.0001M && Math.Abs(l.Longitude - longitude) < 0.0001M);
            return location?.Id ?? 0;  
        }

        private async Task<Image> UploadImageAsync(int reportId, IFormFile file)
        {

            if (file == null || file.Length == 0)
                throw new ArgumentException("File is not valid");

            var reportExists = await _context.Reports.AnyAsync(r => r.Id == reportId);
            if (!reportExists)
                throw new KeyNotFoundException($"Report with ID {reportId} not found.");

            if (string.IsNullOrEmpty(_environment.WebRootPath))
                throw new InvalidOperationException("Web root path is not configured.");

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);


            string fileExtension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            string imagePath = Path.Combine(uploadsFolder, uniqueFileName);
            string relativePath = $"uploads/{uniqueFileName}";

            using (var stream = new FileStream(imagePath, FileMode.Create))
                await file.CopyToAsync(stream);


            var image = new Image
            {
                ReportId = reportId,
                ImagePath = relativePath,
                ImageType = file.ContentType,
                ImageName = Path.GetFileName(file.FileName),
                ImageSize = file.Length,
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            return image;
        }

        // Get All Reports Related to specific user ✅
        public async Task<List<Report>> GetUserReportsAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.");

            return await _context.Reports
                .Where(r => r.UserId == userId)
                .OrderBy(r => r.CreatedAt)
                .Select(report => new Report
                {
                    Id = report.Id,
                    Description = report.Description,
                    ReportType = report.ReportType,
                    CreatedAt = report.CreatedAt,
                    Status = report.Status,
                    UserId = report.UserId,
                    Location = report.Location,
                    Image = report.Image
                })
                .ToListAsync();
        }

    }
}
