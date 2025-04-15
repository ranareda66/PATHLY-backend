using PATHLY_API.Services;
using System.Web;

public class RoadService : IRoadService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IConfiguration _configuration;

	public RoadService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
	{
		_httpClientFactory = httpClientFactory;
		_configuration = configuration;
	}

	public async Task<string> SnapToRoadsAsync(string path)
	{
		var apiKey = _configuration["GoogleMaps:ApiKey"];
		if (string.IsNullOrEmpty(apiKey))
			throw new InvalidOperationException("Google Maps API key is not configured");

		var encodedPath = HttpUtility.UrlEncode(path);
		var url = $"https://roads.googleapis.com/v1/snapToRoads?path={encodedPath}&interpolate=true&key={apiKey}";

		try
		{
			var client = _httpClientFactory.CreateClient();
			var response = await client.GetAsync(url);

			if (!response.IsSuccessStatusCode)
				throw new HttpRequestException($"Google Roads API request failed with status: {response.StatusCode}");

			return await response.Content.ReadAsStringAsync();
		}
		catch (Exception ex)
		{
			throw new Exception("Error calling Google Roads API", ex);
		}
	}
}


