using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;

namespace PATHLY_API.Services
{
    public class SubscriptionPlanService
    {
        private readonly ApplicationDbContext _context;
        public SubscriptionPlanService(ApplicationDbContext context) => _context = context;

        // Get All Subscription Plans in App ✅
        public async Task<List<object>> GetSubscriptionPlansAsync()
        {
            return await _context.SubscriptionPlans
                .Select(plan => new 
                {
                    plan.Id,
                    plan.Name,
                    plan.Description,
                    plan.Price,
                    DurationInMonths = plan.DurationInMonths + " Months",
                    Currency = "USD"
                })
                .ToListAsync<object>();
        }
    }
}