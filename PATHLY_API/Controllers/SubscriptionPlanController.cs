using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Services;

namespace PATHLY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly SubscriptionPlanService _subscriptionService;
        public SubscriptionPlanController(SubscriptionPlanService subscriptionService) => _subscriptionService = subscriptionService;

        // Get All Subscription Plans ✅
        [HttpGet]
        public async Task<IActionResult> GetSubscriptionPlans()
        {
            var plans = await _subscriptionService.GetSubscriptionPlansAsync();
            return Ok(new { plans });
        }
    }
}
