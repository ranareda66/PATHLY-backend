using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Services;

namespace PATHLY_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrafficController : ControllerBase
    {
        private readonly GoogleTrafficService _googleMapsService;
        public TrafficController(GoogleTrafficService googleMapsService) => _googleMapsService = googleMapsService;

        [HttpGet("{placeName}")]
        public async Task<IActionResult> GetTrafficData(string placeName)
        {
            if (string.IsNullOrWhiteSpace(placeName))
                return BadRequest("Place name cannot be empty");

            try
            {
                var trafficData = await _googleMapsService.GetTrafficDataAsync(placeName);
                return Ok(trafficData);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Google Maps API error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        //[HttpGet("{latitude}/{longitude}")]
        //public async Task<IActionResult> GetTrafficData(double latitude, double longitude)
        //{
        //    // Validate coordinates
        //    if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
        //    {
        //        return BadRequest("Invalid coordinates");
        //    }

        //    try
        //    {
        //        var trafficData = await _googleMapsService.GetTrafficDataAsync(latitude, longitude);
        //        return Ok(new
        //        {
        //            Success = true,
        //            Data = trafficData,
        //            Timestamp = DateTime.UtcNow
        //        });
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            Success = false,
        //            Error = "Google Maps API error",
        //            Details = ex.Message
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            Success = false,
        //            Error = "Internal server error",
        //            Details = ex.Message
        //        });
        //    }
        //}

        //[HttpGet("{latitude}/{longitude}")]
        //public async Task<IActionResult> GetTrafficData(double latitude, double longitude)
        //{
        //    // Validate coordinates
        //    if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
        //    {
        //        return BadRequest("Invalid coordinates. Latitude must be between -90 and 90, Longitude between -180 and 180");
        //    }

        //    try
        //    {
        //        var trafficData = await _googleMapsService.GetTrafficDataAsync(latitude, longitude);
        //        return Ok(trafficData);
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        return StatusCode(500, $"Google Maps API error: {ex.Message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}

    }
}
