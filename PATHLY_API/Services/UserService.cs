using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;
using PATHLY_API.Dto;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;


namespace PATHLY_API.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserService(UserManager<User> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }



        // Change Email for user ✅  
        public async Task<string> ChangeEmailAsync(ClaimsPrincipal user, string newEmail, string password)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var appUser = await _userManager.FindByIdAsync(userIdClaim);

            if (userIdClaim is null || appUser is null)
                return "User is not found.";

            var isPasswordValid = await _userManager.CheckPasswordAsync(appUser, password);
            if (!isPasswordValid)
                return "Invalid password.";

            if (appUser.Email == newEmail)
                return "New email cannot be the same as the current email.";

            var existingUser = await _userManager.FindByEmailAsync(newEmail);
            if (existingUser is not null)
                return "Email is already in use.";

            var token = await _userManager.GenerateChangeEmailTokenAsync(appUser, newEmail);
            var result = await _userManager.ChangeEmailAsync(appUser, newEmail, token);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return $"Failed to change email. Errors: {errors}";
            }

            return "Email changed successfully.";
        }

        // Change Password for user ✅
        public async Task<string> ChangePasswordAsync(ClaimsPrincipal user, string currentPassword, string newPassword)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var appUser = await _userManager.FindByIdAsync(userIdClaim);

            if (userIdClaim is null || appUser is null)
                return "User is not found.";

            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(appUser, currentPassword);
            if (!isCurrentPasswordValid)
                return "Current password is incorrect.";

            if (currentPassword == newPassword)
                return "New password cannot be the same as the current password.";

            var result = await _userManager.ChangePasswordAsync(appUser, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return $"Failed to change password. Errors: {errors}";
            }

            return "Password changed successfully.";
        }

        // Get Subscription Status for User ✅
        public async Task<List<object>> GetUserSubscriptionStatusAsync(int userId)
        {
            var subscriptions = await _context.UserSubscriptions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartDate)
                .Select(subscription => new
                {
                    UserId = "User ID: " + userId,
                    PlanId = "Plan: " + subscription.SubscriptionPlanId,
                    Status = subscription.Status.ToString(),
                    Subscription_Start = subscription.StartDate.ToUniversalTime().ToString("yyyy-MM-dd THH:mm:ssZ"),
                    Subscription_end = subscription.EndDate.ToUniversalTime().ToString("yyyy-MM-dd THH:mm:ssZ")
                })
                .ToListAsync<object>();

            return subscriptions;
        }

        // Enable user to Subscribe To Plan without payment  (Temporarily) ✅
        public async Task<string> SubscribeToPlanAsync(ClaimsPrincipal user, int subscriptionPlanId)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var appUser = await _userManager.FindByIdAsync(userIdClaim);

            if (userIdClaim is null || appUser is null)
                return "User is not found.";


            var subscriptionPlan = await _context.SubscriptionPlans.FindAsync(subscriptionPlanId);
            if (subscriptionPlan is null)
                throw new ArgumentNullException(nameof(subscriptionPlan), "Subscription plan not found.");

            var appDbUser = await _context.Users
                .Include(u => u.UserSubscriptions)
                .FirstOrDefaultAsync(u => u.Id.ToString() == userIdClaim);


            if (appDbUser is null)
                throw new Exception("User not found");


            if (subscriptionPlan.DurationInMonths <= 0)
                throw new ArgumentException("Subscription duration must be greater than zero.", nameof(subscriptionPlan.DurationInMonths));


            var newSubscription = new UserSubscription
            {
                UserId = int.Parse(userIdClaim),
                SubscriptionPlanId = subscriptionPlanId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(subscriptionPlan.DurationInMonths),
                Status = SubscriptionStatus.Active,
            };

            _context.UserSubscriptions.Add(newSubscription);
            await _context.SaveChangesAsync();

            return "Subscription successfully created.";
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
                    Distance = t.Distance,
                    Status = t.Status
                })
                .ToListAsync();
        }


    }
}