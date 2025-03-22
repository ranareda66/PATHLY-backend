using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;
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

        public async Task<Report> CreateReportAsync(string reportType,string reportDescription,int userId,IFormFile file,decimal? latitude = null,decimal? longitude = null)
        {
            if (string.IsNullOrWhiteSpace(reportDescription))
                throw new ArgumentException("Report description cannot be empty.", nameof(reportDescription));

            int locationId = 0;

            if (latitude.HasValue && longitude.HasValue && latitude.Value != 0 && longitude.Value != 0)
            {
                locationId = await GetLocationIdByCoordinatesAsync(latitude.Value, longitude.Value);

                if (locationId == 0)
                {
                    // Create New Location if not Exists
                    var newLocation = new Location
                    {
                        Latitude = latitude.Value,
                        Longitude = longitude.Value
                    };

                    _context.Locations.Add(newLocation);
                    await _context.SaveChangesAsync();
                    locationId = newLocation.Id;
                }
            }

            var report = new Report
            {
                ReportType = reportType.Trim(),
                Description = reportDescription.Trim(),
                Status = ReportStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                LocationId = locationId ,
                UserId = userId 
            };

            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();


            if (file != null && file.Length > 0)
                await UploadImageAsync(report.Id, file);

            return report;
        }

        private async Task<int> GetLocationIdByCoordinatesAsync(decimal latitude, decimal longitude)
        {
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => Math.Abs(l.Latitude - latitude) < 0.0001M && Math.Abs(l.Longitude - longitude) < 0.0001M);


            return location?.Id ?? 0;  
        }

        public async Task<Image> UploadImageAsync(int reportId, IFormFile file)
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
    }
}
