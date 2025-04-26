using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Dto;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;
using PATHLY_API.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

public class PaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateSubscriptionAsync(ClaimsPrincipal user, int subscriptionPlanId)
    {
        if (subscriptionPlanId <= 0)
            throw new KeyNotFoundException($"Subscription Plan ID is Invalid");


        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;


        if (!int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedAccessException($"Invalid User ID format. Value received: '{userIdClaim}'");

        var plan = await _context.SubscriptionPlans.FindAsync(subscriptionPlanId);
        if (plan is null)
            throw new KeyNotFoundException("Subscription Plan not found.");


		var newSubscription = new UserSubscription
        {
            UserId = userId,
            SubscriptionPlanId = subscriptionPlanId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(plan.DurationInMonths),
            Status = SubscriptionStatus.Active
        };

		_context.UserSubscriptions.Add(newSubscription);
		await _context.SaveChangesAsync();
    }   
}