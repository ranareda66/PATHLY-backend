using System.Text.Json;

namespace PATHLY_API.Services
{
	public class GooglePlacesService
	{
		private readonly IConfiguration _configuration;
		private readonly HttpClient _httpClient;

		public GooglePlacesService(IConfiguration configuration, HttpClient httpClient)
		{
			_configuration = configuration;
			_httpClient = httpClient;
		}

		public async Task<string> GetNearbyPlacesAsync(double lat, double lng, int radius = 1000, string type = "restaurant")
		{
			var apiKey = _configuration["GoogleMaps:ApiKey"];

			var url = $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={lat},{lng}&radius={radius}&type={type}&key={apiKey}";

			try
			{
				var response = await _httpClient.GetAsync(url);
				response.EnsureSuccessStatusCode();

				var content = await response.Content.ReadAsStringAsync();
				var json = JsonDocument.Parse(content);
				var places = json.RootElement.GetProperty("results")
					.EnumerateArray()
					.Select(place =>
					{
						var photoReference = place.TryGetProperty("photos", out var photos) && photos.GetArrayLength() > 0
							? photos[0].GetProperty("photo_reference").GetString()
							: null;

						var photoUrl = photoReference != null
							? $"https://maps.googleapis.com/maps/api/place/photo?maxwidth=400&photoreference={photoReference}&key={apiKey}"
							: null;

						return new
						{
							name = place.GetProperty("name").GetString(),
							address = place.TryGetProperty("vicinity", out var vicinity) ? vicinity.GetString() : "",
							rating = place.TryGetProperty("rating", out var rating) ? rating.GetDouble() : 0,
							location = new
							{
								lat = place.GetProperty("geometry").GetProperty("location").GetProperty("lat").GetDouble(),
								lng = place.GetProperty("geometry").GetProperty("location").GetProperty("lng").GetDouble()
							},
							photo = photoUrl
						};
					});


				var simplifiedJson = JsonSerializer.Serialize(places);
				return simplifiedJson;
			}
			catch (HttpRequestException ex)
			{
				throw new Exception("Failed to fetch nearby places.", ex);
			}
		}

		public async Task<string> GetAutocompleteSuggestions(string input, double lat, double lng, int radius = 1000)
		{
			var apiKey = _configuration["GoogleMaps:ApiKey"];
			var url = $"https://maps.googleapis.com/maps/api/place/autocomplete/json?location={lat},{lng}&radius={radius}&input={input}&key={apiKey}";

			var response = await _httpClient.GetAsync(url);
			var content = await response.Content.ReadAsStringAsync();
			var json = JsonDocument.Parse(content);

			var predictions = json.RootElement.GetProperty("predictions")
				.EnumerateArray()
				.Select(p => new
				{
					description = p.GetProperty("description").GetString(),
					placeId = p.GetProperty("place_id").GetString()
				});

			var simplifiedJson = JsonSerializer.Serialize(predictions);

			return simplifiedJson;
		}

		public async Task<string> GetPlaceDetails(string placeId)
		{
			var apiKey = _configuration["GoogleMaps:ApiKey"];
			var url = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&key={apiKey}";

			var response = await _httpClient.GetAsync(url);
			var content = await response.Content.ReadAsStringAsync();
			var json = JsonDocument.Parse(content);
			var result = json.RootElement.GetProperty("result");

			var simplified = new
			{
				name = result.GetProperty("name").GetString(),
				address = result.TryGetProperty("formatted_address", out var address) ? address.GetString() : "",
				phone = result.TryGetProperty("formatted_phone_number", out var phone) ? phone.GetString() : "",
				rating = result.TryGetProperty("rating", out var rating) ? rating.GetDouble() : 0,
				location = new
				{
					lat = result.GetProperty("geometry").GetProperty("location").GetProperty("lat").GetDouble(),
					lng = result.GetProperty("geometry").GetProperty("location").GetProperty("lng").GetDouble()
				}
			};

			return JsonSerializer.Serialize(simplified);
		}

	}
}
