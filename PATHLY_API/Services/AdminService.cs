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

        // Disable or activate the user account
        public async Task<bool> ToggleUserStatusAsync(int userId, bool isActive)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null) return false;

            user.IsActive = isActive;
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



        // Retrieve user's Trip history with filtering and pagination
        //public async Task<List<TripDto>> GetUserTripsAsync(int userId, DateTime? StartTime, TripStatus? status)
        //{
        //    var query = _context.Trips.Where(t => t.UserId == userId);

        //    if (StartTime.HasValue)
        //        query = query.Where(t => t.StartTime >= StartTime.Value);

        //    if (status.HasValue)
        //        query = query.Where(t => t.Status == status.Value);

        //    return await query
        //        .OrderByDescending(t => t.StartTime)
        //        .Select(t => new TripDto
        //        {
        //            Id = t.Id,
        //            StartLatitude = t.StartLatitude,
        //            StartLongitude = t.StartLongitude,
                   
        //            EndLatitude = t.EndLatitude,
        //            EndLongitude = t.EndLongitude,
                   


        //            StartTime = t.StartTime,
        //            EndTime = t.EndTime,
        //            Distance = t.Distance,
        //            Status = t.Status.ToString()
        //        })
        //        .ToListAsync();
        //}

        // Add or update road data
        public async Task<bool> AddOrUpdateRoadAsync(RoadDto roadDto)
        {
            var road = await _context.Roads.FindAsync(roadDto.Id) ?? new Road();

            road.Id = roadDto.Id;
            road.Start = roadDto.Start;
            road.Destination = roadDto.Destination;
            road.RoadLength = roadDto.Length;
            road.QualityScore = roadDto.QualityScore;
            road.Quality = roadDto.Quality;
            road.LastUpdate = roadDto.LastUpdate;

            // Adding Road if not exists in DB or Modifying it if exists
            _context.Entry(road).State = road.Id == 0 ? EntityState.Added : EntityState.Modified;

            await _context.SaveChangesAsync();
            return true;
        }

        // Modify road quality
        public async Task<bool> UpdateRoadQualityAsync(int roadId, RoadQuality quality)
        {
            var road = await _context.Roads.FindAsync(roadId);
            if (road is null) return false;

            road.Quality = quality;
            await _context.SaveChangesAsync();
            return true;
        }

        // Update Quality Metrics For Roads
        public bool UpdateQualityMetrics(int roadId, QualityMetric updatedQualityMetric)
        {
            var road = _context.Roads.FirstOrDefault(r => r.Id == roadId);
            if (road is null)
                throw new ArgumentException("Road not found.", nameof(roadId));

            if (road.QualityMetric == null)
                throw new InvalidOperationException("Road does not have a quality metric.");

            bool isUpdated = false;

            foreach (var property in typeof(QualityMetric).GetProperties())
            {
                var oldValue = property.GetValue(road.QualityMetric);
                var newValue = property.GetValue(updatedQualityMetric);

                if (newValue is not null && !newValue.Equals(oldValue))
                {
                    property.SetValue(road.QualityMetric, newValue);
                    isUpdated = true;
                }
            }

            return isUpdated ? _context.SaveChanges() > 0 : false;
        }
    }
}






