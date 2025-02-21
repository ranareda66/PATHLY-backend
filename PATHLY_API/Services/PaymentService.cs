using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Dto;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;
using PATHLY_API.Services;
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

	public async Task<PaymentResponse> ProcessPayPalPaymentAsync(int userId, int subscriptionPlanId)
	{
		var user = await _context.Users.FindAsync(userId);
		var plan = await _context.SubscriptionPlans.FindAsync(subscriptionPlanId);

		if (user == null || plan == null)
			throw new KeyNotFoundException("User or Subscription Plan not found.");

		// Create PayPal order
		string orderId = await _payPalService.CreateOrderAsync(plan.Price, "USD");

		// Store payment in database as "Pending"
		var payment = new Payment
		{
			UserId = userId,
			SubscriptionPlanId = subscriptionPlanId,
			Amount = plan.Price,
			PaymentStatus = PaymentStatus.Pending,
			PaymentMethod = "PayPal",
			PaymentDate = DateTime.UtcNow,
			TransactionId = orderId // Store PayPal Order ID
		};

		_context.Payments.Add(payment);
		await _context.SaveChangesAsync();

		// Return PayPal Order ID and Approval URL
		return new PaymentResponse
		{
			OrderId = orderId,
			ApprovalUrl = $"https://www.sandbox.paypal.com/checkoutnow?token={orderId}"
		};
	}

	public async Task<bool> CompletePayPalPaymentAsync(string orderId)
	{
		using (var transaction = await _context.Database.BeginTransactionAsync())
		{
			try
			{
				var payment = await _context.Payments.FirstOrDefaultAsync(p => p.TransactionId == orderId);
				if (payment == null) throw new KeyNotFoundException("Payment not found.");

				bool success = await _payPalService.CapturePaymentAsync(orderId);

				if (success)
				{
					payment.PaymentStatus = PaymentStatus.Completed;

					// Activate Subscription
					var userSubscription = new UserSubscription
					{
						UserId = payment.UserId,
						SubscriptionPlanId = payment.SubscriptionPlanId,
						StartDate = DateTime.UtcNow,
						EndDate = DateTime.UtcNow.AddMonths(1) // Example: 1 month subscription
					};
					_context.UserSubscriptions.Add(userSubscription);

					await _context.SaveChangesAsync();
					await transaction.CommitAsync();
				}
				else
				{
					payment.PaymentStatus = PaymentStatus.Cancelled;
					await _context.SaveChangesAsync();
					await transaction.RollbackAsync();
				}

				return success;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				Console.WriteLine($"Error: {ex.Message}");
				throw;
			}
		}
	}
}
