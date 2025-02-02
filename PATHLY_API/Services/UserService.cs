using PATHLY_API.Data;
using PATHLY_API.Models;

namespace PATHLY_API.Services
{
	public class UserService
	{
		private readonly ApplicationDbContext _context;

		public UserService(ApplicationDbContext context)
		{
			_context = context;
		}
		public async Task AbortTripAsync(int userId, int tripId)
		{
			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				throw new Exception("User not found");
			}

			// Perform business logic
			user.TripCount--;

			// Save changes to the database
			await _context.SaveChangesAsync();
		}
		public async Task SubscribeToPlanAsync(int userId, SubscriptionPlan subscriptionPlan)
		{
			if (subscriptionPlan == null)
			{
				throw new ArgumentNullException(nameof(subscriptionPlan), "Subscription plan cannot be null.");
			}

			if (subscriptionPlan.DurationInMonths <= 0)
			{
				throw new ArgumentException("Subscription duration must be greater than zero.", nameof(subscriptionPlan.DurationInMonths));
			}

			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				throw new Exception("User not found");
			}

			// Check if the user is already subscribed
			if (user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate > DateTime.UtcNow)
			{
				// Option 1: Extend the subscription
				user.SubscriptionEndDate = user.SubscriptionEndDate.Value.AddMonths(subscriptionPlan.DurationInMonths);
			}
			else
			{
				// Option 2: Start a new subscription
				user.SubscriptionStatus = "Active";
				user.SubscriptionStartDate = DateTime.UtcNow;
				user.SubscriptionEndDate = DateTime.UtcNow.AddMonths(subscriptionPlan.DurationInMonths);
			}

			// Save changes to the database
			await _context.SaveChangesAsync();
		}
		public async Task UpdateSubscriptionStatusAsync(int userId, string newStatus)
		{
			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				throw new KeyNotFoundException($"User with ID {userId} not found.");
			}

			var validStatuses = new HashSet<string> { "Active", "Inactive", "Canceled", "Trial", "Expired" };
			if (!validStatuses.Contains(newStatus))
			{
				throw new ArgumentException($"Invalid subscription status: {newStatus}");
			}

			// Perform business logic
			user.SubscriptionStatus = newStatus;

			// Save changes to the database
			await _context.SaveChangesAsync();
		}
		public async Task IncrementTripCountAsync(int userId)
		{
			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				throw new Exception("User not found");
			}

			// Perform business logic
			user.TripCount++;

			// Save changes to the database
			await _context.SaveChangesAsync();
		}
		public async Task PauseTripAsync(int userId, int tripId)
		{
			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				throw new Exception("User not found");
			}

			// Perform business logic (e.g., update trip status)
			// Note: You'll need a Trip class and repository for this.

			// Save changes to the database
			await _context.SaveChangesAsync();
		}
	}
}
