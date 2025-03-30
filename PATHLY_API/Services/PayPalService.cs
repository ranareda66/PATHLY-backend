using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PATHLY_API.Data;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalEnvironment = PayPalCheckoutSdk.Core.PayPalEnvironment;

namespace PATHLY_API.Services
{
	public class PayPalService
	{
		private readonly PayPalHttpClient _client;

		public PayPalService(IOptions<PayPalSettings> payPal )
		{
            var _payPal = payPal.Value;
			PayPalEnvironment environment = _payPal.Environment.ToString().ToLower() switch
			{
				"live" => new LiveEnvironment(_payPal.ClientId, _payPal.Secret),
				_ => new SandboxEnvironment(_payPal.ClientId, _payPal.Secret) // Default to Sandbox
			};
			_client = new PayPalHttpClient(environment);
		}

		    public async Task<string> CreateOrderAsync(decimal? amount, string currency)
		    {

                if (amount is null || amount <= 0)
                    throw new ArgumentException("Invalid amount for payment.");

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
				       	      Value = amount?.ToString("F2") // Format to 2 decimal places
                           }
				       }
			        }
			    };

			    var request = new OrdersCreateRequest();
			    request.Prefer("return=representation");
			    request.RequestBody(order);

                try
                {
                    var response = await _client.Execute(request);
                    if (response.StatusCode != System.Net.HttpStatusCode.Created)
                        throw new Exception($"PayPal order creation failed. Status Code: {response.StatusCode}");

                    return response.Result<Order>().Id;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error while creating PayPal order: {ex.Message}", ex);
                }
            }

        public async Task<bool> CaptureOrderAsync(string orderId)
        {
            var getRequest = new OrdersGetRequest(orderId);
            var getResponse = await _client.Execute(getRequest);
            var order = getResponse.Result<Order>();

            Console.WriteLine($"Order ID: {order.Id}, Status: {order.Status}");

            if (order.Status != "APPROVED")
                throw new InvalidOperationException("Order must be in APPROVED state before capture.");

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
