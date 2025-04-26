using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Models;
using PATHLY_API.Services;

namespace PATHLY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapsController : ControllerBase
    {
        private readonly GoogleTrafficService _mapsService;
        private readonly SearchService _searchService;

        public MapsController(GoogleTrafficService mapsService , SearchService searchService)
        {
            _mapsService = mapsService;
            _searchService = searchService;
        }

        [HttpGet("get-address")]
        public async Task<IActionResult> GetAddress([FromQuery] decimal latitude, [FromQuery] decimal longitude)
        {
            if (latitude == 0 || longitude == 0)
                return BadRequest("Invalid coordinates.");

            var address = await _mapsService.GetAddressFromCoordinates(latitude, longitude);
            return Ok(new { Address = address });

        }

        [HttpPost("get-distance-by-name")]
        public async Task<IActionResult> GetDistanceByName([FromBody] DistanceByNameModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Origin) || string.IsNullOrWhiteSpace(model.Destination))
                return BadRequest("Both origin and destination must be provided.");

            var distance = await _mapsService.GetDistanceBetweenPlacesByName(model.Origin, model.Destination);

            if (distance == -1)
                return BadRequest("Unable to calculate distance between the provided places.");

            return Ok(new { DistanceInKm = distance });
        }

    }
}
