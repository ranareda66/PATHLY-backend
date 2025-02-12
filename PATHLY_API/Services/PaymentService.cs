using PATHLY_API.Data;
using PATHLY_API.Dto;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;
using Serilog;

public class PaymentService
{
	private readonly ApplicationDbContext _context;
	private readonly PayPalService _payPalService;

	public PaymentService(ApplicationDbContext context, PayPalService payPalService)
	{
		_context = context;
		_payPalService = payPalService;
	}

	public async Task<string> ProcessPayPalPaymentAsync(int userId, int subscriptionPlanId)
	{
		var user = await _context.Users.FindAsync(userId);
		var plan = await _context.SubscriptionPlans.FindAsync(subscriptionPlanId);

		if (user == null || plan == null)
			throw new KeyNotFoundException("User or Subscription Plan not found.");

		// Create PayPal order
		string orderId = await _payPalService.CreateOrderAsync(plan.Price);

		// Store payment in database as "Pending"
		var payment = new Payment
		{
			UserId = userId,
			SubscriptionPlanId = subscriptionPlanId,
			Amount = plan.Price,
			PaymentStatus = PaymentStatus.Pending,
			PaymentMethod = "PayPal",
			PaymentDate = DateTime.UtcNow
		};

		_context.Payments.Add(payment);
		await _context.SaveChangesAsync();

		return orderId;  // Return PayPal Order ID
	}

	public async Task<bool> CompletePayPalPaymentAsync(int paymentId, string orderId)
	{
		var payment = await _context.Payments.FindAsync(paymentId);
		if (payment == null) throw new KeyNotFoundException("Payment not found.");

		bool success = await _payPalService.CapturePaymentAsync(orderId);

		if (success)
		{
			payment.PaymentStatus = PaymentStatus.Completed;

			// Activate Subscription
			var userSubscription = new UserSubscription
            {
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow.AddMonths(1) // Example: 1 month subscription
			};
			_context.UserSubscriptions.Add(userSubscription);

			await _context.SaveChangesAsync();
			Log.Information($"Subscription activated for User {payment.UserId}");
		}
		else
		{
			payment.PaymentStatus = PaymentStatus.Cancelled;
			await _context.SaveChangesAsync();
		}

		return success;
	}
	//public async Task<bool> RefundPaymentAsync(int paymentId, string saleId, decimal amount)
	//{
	//	var payment = await _context.Payments.FindAsync(paymentId);
	//	if (payment == null) throw new KeyNotFoundException("Payment not found.");

	//	bool success = await _payPalService.RefundPaymentAsync(saleId, amount);

	//	if (success)
	//	{
	//		payment.PaymentStatus = PaymentStatus.Cancelled;
	//		await _context.SaveChangesAsync();
	//	}
	//	else
	//	{
	//		payment.PaymentStatus = PaymentStatus.Cancelled;
	//		await _context.SaveChangesAsync();
	//	}

	//	return success;
	//}

}
