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
    private readonly PayPalService _payPalService;

    public PaymentService(ApplicationDbContext context, PayPalService payPalService)
    {
        _context = context;
        _payPalService = payPalService;
    }

    public async Task<string> CreatePaymentAsync(ClaimsPrincipal user, int subscriptionPlanId)
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

        string orderId;
        try
        {
            orderId = await _payPalService.CreateOrderAsync(plan.Price, "USD");

            if (string.IsNullOrEmpty(orderId))
                throw new Exception("Failed to create PayPal order. No Order ID returned.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while creating PayPal order: {ex.Message}", ex);
        }


        var payment = new Payment
        {
            UserId = userId,
            SubscriptionPlanId = subscriptionPlanId,
            Amount = plan.Price,
            PaymentMethod = PaymentMethodType.PayPal,
            TransactionId = orderId
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Return PayPal Order ID and Approval URL
        return orderId;
    }


    public async Task<bool> CapturePaymentAsync(string orderId)
    {
        var payment = await _context.Payments
            .Include(p => p.SubscriptionPlan)
            .FirstOrDefaultAsync(p => p.TransactionId == orderId);

        if (payment == null)
            throw new KeyNotFoundException("Payment not found.");

        bool success = await _payPalService.CaptureOrderAsync(orderId);

        if (success)
        {
            payment.PaymentStatus = PaymentStatus.Completed;

            var subscriptionPlan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(sp => sp.Id == payment.SubscriptionPlanId);

            if (subscriptionPlan == null)
                throw new KeyNotFoundException("Subscription plan not found.");

            // Determine subscription duration (only monthly or annual)
            int durationMonths = subscriptionPlan.DurationInMonths == 12 ? 12 : 1;

            // Check if user already has an active subscription
            var latestSubscription = await _context.UserSubscriptions
                .Where(us => us.UserId == payment.UserId && us.Status == SubscriptionStatus.Active)
                .OrderByDescending(us => us.EndDate)
                .FirstOrDefaultAsync();

            if (latestSubscription != null && latestSubscription.EndDate > DateTime.UtcNow)
            {
                // Only extend if it's the same plan type
                if (latestSubscription.SubscriptionPlanId == payment.SubscriptionPlanId)
                    // Extend current subscription
                    latestSubscription.EndDate = latestSubscription.EndDate.AddMonths(durationMonths);
                else
                {
                    // Create a new subscription (don't extend the existing one)
                    var newSubscription = new UserSubscription
                    {
                        UserId = payment.UserId,
                        SubscriptionPlanId = payment.SubscriptionPlanId,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(durationMonths),
                        Status = SubscriptionStatus.Active
                    };
                    _context.UserSubscriptions.Add(newSubscription);

                    // Optionally: Cancel the previous subscription immediately or let it expire naturally
                    // latestSubscription.Status = SubscriptionStatus.Cancelled;
                }
            }
            else
            {
                // No active subscription, create a new one
                var newSubscription = new UserSubscription
                {
                    UserId = payment.UserId,
                    SubscriptionPlanId = payment.SubscriptionPlanId,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(durationMonths),
                    Status = SubscriptionStatus.Active
                };
                _context.UserSubscriptions.Add(newSubscription);
            }

            // Update user's subscription status
            var user = await _context.Users.FindAsync(payment.UserId);
            if (user != null)
            {
                user.SubscriptionStatus = SubscriptionStatus.Active;
            }

            await _context.SaveChangesAsync();
        }
        else
        {
            payment.PaymentStatus = PaymentStatus.Cancelled;
            await _context.SaveChangesAsync();
        }

        return success;
    }

    public async Task<bool> CancelPaymentAsync(string orderId)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.TransactionId == orderId);
        if (payment is null) throw new KeyNotFoundException("Payment not found.");

        payment.PaymentStatus = PaymentStatus.Cancelled;
        await _context.SaveChangesAsync();

        return true;
    }


}