using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Dto;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;

namespace PATHLY_API.Services
{
    public class AdminService
    {
        private readonly ApplicationDbContext _context;
        public AdminService(ApplicationDbContext context) => _context = context;

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
                    Latitude = report.Latitude,
                    Longitude = report.Longitude,
                    Image = report.Image
                })
                .ToListAsync();
        }

        // Return Reports based on report status ✅
        public async Task<List<Report>> GetReportsByStatusAsync(ReportStatus? status)
        {
            var query = _context.Reports.AsQueryable();  

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value); 

            return await query
                .OrderBy(r => r.CreatedAt)
                .Select(report => new Report
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
                })
                .ToListAsync();
        }

        // Update the report status (approval or rejection) ✅
        public async Task<bool> UpdateReportStatusAsync(int reportId, ReportStatus newStatus)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report is null) return false;

            report.Status = newStatus;
            _context.Entry(report).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        // Search for users (with the ability to search by name or email)
        public async Task<List<UserDto>> SearchUsersAsync(string? name, string? email)
        {
            var query = _context.Users.AsNoTracking();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(u => u.UserName.Contains(name));

            if (!string.IsNullOrEmpty(email))
                query = query.Where(u => u.Email.Contains(email));

            return await query.Select(user => new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            }).ToListAsync();
        }

        // Edit user data
        public async Task<bool> UpdateUserAsync(int userId, UserDto updatedUser)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null) return false;

            user.UserName = updatedUser.UserName;
            user.Email = updatedUser.Email;
            user.IsActive = updatedUser.IsActive;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Delete User
        public async Task<bool> DeleteAccountAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}






