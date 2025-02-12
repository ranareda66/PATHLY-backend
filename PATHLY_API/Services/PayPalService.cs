using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalCheckoutSdk.Payments;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog;

public class PayPalService
{
	private readonly PayPalEnvironment _environment;
	private readonly PayPalHttpClient _client;

	public PayPalService(IConfiguration configuration)
	{
		var clientId = configuration["PayPal:ClientId"];
		var clientSecret = configuration["PayPal:ClientSecret"];
		var mode = configuration["PayPal:Mode"];

		_environment = mode == "live"
			? new LiveEnvironment(clientId, clientSecret)
			: new SandboxEnvironment(clientId, clientSecret);

		_client = new PayPalHttpClient(_environment);
	}

	public async Task<string> CreateOrderAsync(decimal amount)
	{
		var orderRequest = new OrderRequest()
		{
			CheckoutPaymentIntent = "CAPTURE",
			PurchaseUnits = new List<PurchaseUnitRequest>()
			{
				new PurchaseUnitRequest()
				{
					AmountWithBreakdown = new AmountWithBreakdown()
					{
						CurrencyCode = "EGP",
						Value = amount.ToString("F2")
					}
				}
			}
		};

		var request = new OrdersCreateRequest();
		request.Prefer("return=representation");
		request.RequestBody(orderRequest);

		var response = await _client.Execute(request);
		var result = response.Result<Order>();

		Log.Information($"PayPal order created: {result.Id}");
		return result.Id;  // Return the PayPal Order ID
	}

	public async Task<bool> CapturePaymentAsync(string orderId)
	{
		var request = new OrdersCaptureRequest(orderId);
		request.RequestBody(new OrderActionRequest());

		var response = await _client.Execute(request);
		var result = response.Result<Order>();

		return result.Status == "COMPLETED";
	}

	//public async Task<bool> RefundPaymentAsync(string saleId, decimal amount)
	//{
	//	try
	//	{
	//		// Create a RefundRequest
	//		var refundRequest = new RefundRequest()
	//		{
	//			Amount = new PayPalCheckoutSdk.Payments.Money()
	//			{
	//				CurrencyCode = "EGP",
	//				Value = amount.ToString("F2")  // Ensure the amount is formatted to 2 decimal places
	//			}
	//		};

	//		var request = new CapturesRefundRequest(saleId);
	//		request.RequestBody(refundRequest);

	//		// Execute the refund request
	//		var response = await _client.Execute(request);
	//		var result = response.Result<PayPalCheckoutSdk.Payments.Refund>();

	//		return result.Status == "completed";  // Check if refund was successful
	//	}
	//	catch (Exception ex)
	//	{
	//		// Log the exception and return false
	//		Console.WriteLine($"Error during refund: {ex.Message}");
	//		return false;
	//	}
	//}

}
