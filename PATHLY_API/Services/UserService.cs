using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;
using PATHLY_API.Dto;


namespace PATHLY_API.Services
{
	public class UserService
	{
		private readonly ApplicationDbContext _context;

		public UserService(ApplicationDbContext context) => _context = context;

        public async Task SubscribeToPlanAsync(int userId, int subscriptionPlanId)
        {
            var subscriptionPlan = await _context.SubscriptionPlans.FindAsync(subscriptionPlanId);
            if (subscriptionPlan == null)
                throw new ArgumentNullException(nameof(subscriptionPlan), "Subscription plan not found.");

            if (subscriptionPlan.DurationInMonths <= 0) 
                throw new ArgumentException("Subscription duration must be greater than zero.", nameof(subscriptionPlan.DurationInMonths));

            var user = await _context.Users
                .Include(u => u.UserSubscriptions)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found");

            var activeSubscription = user.UserSubscriptions
                .Where(us => us.Status == SubscriptionStatus.Active)
                .FirstOrDefault();

            if (activeSubscription != null)
                throw new Exception("User already has an active subscription.");

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

        // Update User Data
        public async Task<bool> UpdateUserAsync(int userId, string newEmail)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            if (user.Email == newEmail) 
                return false;

            user.Email = newEmail;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Change User Password
        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !BCrypt.Net.BCrypt.Verify(oldPassword, user.Password) || BCrypt.Net.BCrypt.Verify(newPassword, user.Password))
                return false;

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Need to Edit where user can remove his account , but not delete himself from database
        public async Task<bool> DeleteAccountAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
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

    }
}
