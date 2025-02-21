using Microsoft.Extensions.Options;
using PATHLY_API.Models;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalCheckoutSdk.Payments;

namespace PATHLY_API.Services
{
	public class PayPalService
	{
		private readonly PayPalHttpClient _client;

		public PayPalService(IOptions<PayPalSettings> payPal)
		{
			var _payPal = payPal.Value;
			var environment = new SandboxEnvironment(_payPal.ClientId, _payPal.Secret);
			_client = new PayPalHttpClient(environment);
		}

		public async Task<string> CreateOrderAsync(decimal amount, string currency)
		{
			var order = new OrderRequest
			{
				CheckoutPaymentIntent = "CAPTURE",
				ApplicationContext = new ApplicationContext
				{
					ReturnUrl = "https://localhost:7098/api/Payment/success", // Redirect after approval
					CancelUrl = "https://localhost:7098/api/Payment/cancel"    // Redirect if cancelled
				},
				PurchaseUnits = new List<PurchaseUnitRequest>
			    {
				   new PurchaseUnitRequest
				   {
				       AmountWithBreakdown = new AmountWithBreakdown
				       {
				       	  CurrencyCode = currency,
				       	  Value = amount.ToString("F2") // Format to 2 decimal places
                       }
				   }
			    }
			};

			var request = new OrdersCreateRequest();
			request.Prefer("return=representation");
			request.RequestBody(order);

			var response = await _client.Execute(request);
			return response.Result<Order>().Id; // Return PayPal Order ID
		}

		public async Task<bool> CapturePaymentAsync(string orderId)
		{
			var request = new OrdersCaptureRequest(orderId);
			try
			{
				var response = await _client.Execute(request);
				return response.StatusCode == System.Net.HttpStatusCode.OK;
			}
			catch (PayPalHttp.HttpException ex)
			{
				// Log the error for debugging
				Console.WriteLine($"PayPal API Error: {ex.Message}");
				throw; 
			}
		}
	}
}
