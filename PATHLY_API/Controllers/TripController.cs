using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Services;
using PATHLY_API.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using PATHLY_API.Models.Enums;
using System.Security.Claims;

namespace PATHLY_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TripController : ControllerBase
    {
        private readonly TripService _tripService;
        private readonly ILogger<TripController> _logger;

        public TripController(
            TripService tripService,
            ILogger<TripController> logger)
        {
            _tripService = tripService;
            _logger = logger;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartTrip([FromBody] StartTripRequest request)
        {
            try
            {
                var result = await _tripService.StartTripWithCoordinatesAsync(
                    request.StartLatitude,
                    request.StartLongitude,
                    request.EndLatitude,
                    request.EndLongitude,
                    request.RequestRoadPrediction);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized trip access");
                return Unauthorized(new ErrorResponse
                {
                    Message = ex.Message,
                    ErrorCode = "REQUIRES_SUBSCRIPTION",
                    ActionRequired = "subscribe",
                    SubscribeUrl = "/api/subscription/plans"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting trip");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while processing your trip",
                    ErrorCode = "TRIP_ERROR"
                });
            }
        }

        [HttpPost("end/{tripId}")]
        public async Task<IActionResult> EndTrip(int tripId, [FromQuery] int? feedback = null)
        {
            try
            {
                var success = await _tripService.EndTripAsync(tripId, feedback);
                return Ok(new { Success = success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending trip");
                return HandleException(ex);
            }
        }

        
        /// Aborts an ongoing trip
        [HttpPost("abort/{tripId}")]
        public async Task<IActionResult> AbortTrip(int tripId)
        {
            try
            {
                var success = await _tripService.AbortTripAsync(tripId);
                return Ok(new { Success = success, Message = "Trip aborted successfully" });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// Deletes a cancelled trip
        [HttpDelete("{tripId}")]
        public async Task<IActionResult> DeleteTrip(int tripId)
        {
            try
            {
                var success = await _tripService.DeleteTripAsync(tripId);
                return Ok(new { Success = success, Message = "Trip deleted successfully" });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

      
        /// Gets trip history for the authenticated user
        [HttpGet("history")]
        public async Task<IActionResult> GetTripHistory(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] TripStatus? status = null)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var trips = await _tripService.GetUserTripsAsync(userId, startDate, status);
                return Ok(trips);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

       
        /// Gets details for a specific trip
        [HttpGet("{tripId}")]
        public async Task<IActionResult> GetTripDetails(int tripId)
        {
            try
            {
                var trip = await _tripService.GetTripDetailsAsync(tripId);
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }



        private IActionResult HandleException(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => Unauthorized(new ErrorResponse
                {
                    Message = ex.Message,
                    ErrorCode = "UNAUTHORIZED"
                }),
                KeyNotFoundException => NotFound(new ErrorResponse
                {
                    Message = ex.Message,
                    ErrorCode = "NOT_FOUND"
                }),
                ArgumentException or ArgumentOutOfRangeException => BadRequest(new ErrorResponse
                {
                    Message = ex.Message,
                    ErrorCode = "INVALID_INPUT"
                }),
                InvalidOperationException => BadRequest(new ErrorResponse
                {
                    Message = ex.Message,
                    ErrorCode = "INVALID_OPERATION"
                }),
                _ => StatusCode(500, new ErrorResponse
                {
                    Message = "An unexpected error occurred",
                    ErrorCode = "INTERNAL_ERROR"
                })
            };
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string ActionRequired { get; set; }
        public string SubscribeUrl { get; set; }
    }
}