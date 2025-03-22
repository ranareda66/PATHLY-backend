using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;
using PATHLY_API.Dto;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;


namespace PATHLY_API.Services
{
    public class UserService : ControllerBase
	{
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserService(UserManager<User> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SubscribeToPlanAsync(int userId, int subscriptionPlanId)
        {
            var subscriptionPlan = await _context.SubscriptionPlans.FindAsync(subscriptionPlanId);
            if (subscriptionPlan == null)
                throw new ArgumentNullException(nameof(subscriptionPlan), "Subscription plan not found.");

            if (subscriptionPlan.DurationInMonths <= 0) 
                throw new ArgumentException("Subscription duration must be greater than zero.", nameof(subscriptionPlan.DurationInMonths));

            var user = await _context.Users
                .Include(u => u.UserSubscription)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found");

            var newSubscription = new UserSubscription
			{
				UserId = userId,
				SubscriptionPlanId = subscriptionPlanId,
				Status = SubscriptionStatus.Active,
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(subscriptionPlan.DurationInMonths)
            };

            _context.UserSubscriptions.Add(newSubscription);
            await _context.SaveChangesAsync();
        }

        public async Task<string> ChangeEmailAsync(string userId, string newEmail, string password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return "User not found.";

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
                return "Invalid password.";

            var existingUser = await _userManager.FindByEmailAsync(newEmail);
            if (existingUser != null)
                return "Email is already in use.";

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var result = await _userManager.ChangeEmailAsync(user, newEmail, token);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return $"Failed to change email. Errors: {errors}";
            }

            return "Email changed successfully.";

        }

        public async Task<string> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return "User not found.";

            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, currentPassword);
            if (!isCurrentPasswordValid)
                return "Current password is incorrect.";

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return $"Failed to change password. Errors: {errors}";
            }

            return "Password changed successfully.";
        }

        // Retrieve user's Trip Details
        public TripDto GetTripDetails(int tripId)
        {
            var trip = _context.Trips.FirstOrDefault(t => t.Id == tripId);
            if (trip == null)
                throw new KeyNotFoundException("Trip not found.");

            return new TripDto
            {
                StartLocation = trip.StartLocation,
                EndLocation = trip.EndLocation,
                Distance = trip.Distance,
                StartTime = trip.StartTime,
                EndTime = trip.EndTime,
                Status = trip.Status,
            };
        }

        // Retrieve user's Trip history with filtering and pagination
        public async Task<List<TripDto>> GetUserTripsAsync(int userId, DateTime? StartTime, TripStatus? status)
        {
            var query = _context.Trips.Where(t => t.UserId == userId);

            if (StartTime.HasValue)
                query = query.Where(t => t.StartTime >= StartTime.Value);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            return await query
                .OrderByDescending(t => t.StartTime)
                .Select(t => new TripDto
                {
                    StartLocation = t.StartLocation,
                    EndLocation = t.EndLocation,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    Distance = t.Distance,
                    Status = t.Status
                })
                .ToListAsync();
        }


        // Get All Reports Related to specific user
        public async Task<List<ReportDto>> GetUserReportsAsync(int userId)
        {


            var reports = await _context.Reports
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();


            return reports.Select(report => new ReportDto
            {
                Description = report.Description,
                ReportType = report.ReportType,
                CreatedAt = report.CreatedAt,
                Status = report.Status,
                Image = report.Image
            }).ToList();
        }


    }
}
