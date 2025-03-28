
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Services;

namespace PATHLY_API.Controllers
{
    [Route("api/subscriptions")]
    [ApiController]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly SubscriptionPlanService _subscriptionService;

        public SubscriptionPlanController(SubscriptionPlanService subscriptionService)
            => _subscriptionService = subscriptionService;


        [HttpGet("plans")]
        public async Task<IActionResult> GetSubscriptionPlans()
        {
            var plans = await _subscriptionService.GetSubscriptionPlansAsync();
            return Ok(new { plans });
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetUserSubscriptionStatus([FromQuery] int userId)
        {
            var status = await _subscriptionService.GetUserSubscriptionStatusAsync(userId);
            return Ok(new { subscriptions = status });
        }
    }
}
