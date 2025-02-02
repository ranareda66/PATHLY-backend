using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalCheckoutSdk.Payments;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

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
}
