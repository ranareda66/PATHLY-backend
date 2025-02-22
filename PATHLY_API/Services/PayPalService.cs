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
			PayPalEnvironment environment = _payPal.Environment.ToLower() switch
			{
				"live" => new LiveEnvironment(_payPal.ClientId, _payPal.Secret),
				_ => new SandboxEnvironment(_payPal.ClientId, _payPal.Secret) // Default to Sandbox
			};
			_client = new PayPalHttpClient(environment);
		}

		public async Task<string> CreateOrderAsync(decimal amount, string currency)
		{
			var order = new OrderRequest
			{
				CheckoutPaymentIntent = "CAPTURE",
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
			var getRequest = new OrdersGetRequest(orderId);
			var getResponse = await _client.Execute(getRequest);
			var order = getResponse.Result<Order>();

			Console.WriteLine($"Order ID: {order.Id}, Status: {order.Status}");

			if (order.Status != "APPROVED")
			{
				throw new InvalidOperationException("Order must be in APPROVED state before capture.");
			}

			var request = new OrdersCaptureRequest(orderId);

			try
			{
				var response = await _client.Execute(request);

				Console.WriteLine($"PayPal Response Status: {response.StatusCode}");

				return response.StatusCode == System.Net.HttpStatusCode.Created;
			}
			catch (PayPalHttp.HttpException ex)
			{
				Console.WriteLine($"PayPal API Error: {ex.Message}");
				throw;
			}
		}
	}
}
