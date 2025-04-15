using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Services;

namespace PATHLY_API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PlacesController : ControllerBase
	{
		private readonly GooglePlacesService _placesService;
		public PlacesController(GooglePlacesService placesService) => _placesService = placesService;


		[HttpGet("nearby")]
		public async Task<IActionResult> GetNearbyPlaces(double lat, double lng, int radius = 1000, string type = "restaurant")
		{
			if (radius <= 0 || radius > 50000)
				return BadRequest("Radius must be between 1 and 50,000 meters.");

			try
			{
				var placesJson = await _placesService.GetNearbyPlacesAsync(lat, lng, radius, type);
				return Content(placesJson, "application/json");
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error fetching places: {ex.Message}");
			}
		}

		[HttpGet("autocomplete")]
		public async Task<IActionResult> GetAutocomplete(string input, double lat, double lng, int radius = 1000)
		{
			var suggestions = await _placesService.GetAutocompleteSuggestions(input, lat, lng, radius);
			return Content(suggestions, "application/json");
		}

		[HttpGet("place-details")]
		public async Task<IActionResult> GetPlaceDetails(string placeId)
		{
			var details = await _placesService.GetPlaceDetails(placeId);
			return Content(details, "application/json");
		}
	}
}
