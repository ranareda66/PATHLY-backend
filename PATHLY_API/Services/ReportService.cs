using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;

namespace PATHLY_API.Services
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ReportService(ApplicationDbContext context) => _context = context;


        public async Task<Report> CreateReportAsync(string reportType, string reportDescription, List<IFormFile> files)
        {
            if (string.IsNullOrWhiteSpace(reportDescription))
                throw new ArgumentException("Report description cannot be empty.", nameof(reportDescription));

            var report = new Report
            {
                ReportType = reportType.Trim(),
                Description = reportDescription.Trim(),
                Status = ReportStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync(); // Save the report first to get ReportId

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                    await UploadImageAsync(report.Id, file); // Upload images and link them to the report
            }

            return report;
        }

        public async Task<Image> UploadImageAsync(int reportId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is not valid");

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            string imagePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(imagePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var image = new Image
            {
                ReportId = reportId,
                ImagePath = imagePath,
                ImageType = file.ContentType,
                ImageName = file.FileName,
                ImageSize = file.Length,
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            return image;
        }
    }
}
