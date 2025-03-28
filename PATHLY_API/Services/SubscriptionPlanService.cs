using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Models.Enums;

namespace PATHLY_API.Services
{
    public class SubscriptionPlanService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionPlanService(ApplicationDbContext context) => _context = context;

        public async Task<List<object>> GetSubscriptionPlansAsync()
        {
            return await _context.SubscriptionPlans.Select(plan => new
            {
                id = "plan_" + plan.Id,
                name = plan.Name,
                description = plan.Description,
                price = plan.Price,
                currency = "USD",
                durationInMonths = plan.DurationInMonths + " months"
            }).ToListAsync<object>();
        }

        public async Task<List<object>> GetUserSubscriptionStatusAsync(int userId)
        {
            var subscriptions = await _context.UserSubscriptions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartDate)
                .Select(subscription => new
                {
                    userId = "user_" + userId,
                    planId = "plan_" + subscription.SubscriptionPlanId,
                    status = subscription.Status.ToString().ToLower(),
                    subscription_start = subscription.StartDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    subscription_end = subscription.EndDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
                })
                .ToListAsync<object>();

            return subscriptions;
        }
    }
}