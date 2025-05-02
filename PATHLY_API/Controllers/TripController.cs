using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Data;
using PATHLY_API.Dto;
using PATHLY_API.Models;
using PATHLY_API.Services;
using System.Security.Claims;


using PATHLY_API.Models.Enums;
using PATHLY_API.Interfaces;


namespace PATHLY_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ILogger<TripController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TripController(
            ITripService tripService,
            ILogger<TripController> logger,
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _tripService = tripService;
            _logger = logger;
            _context = context;
            _env = env;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartTrip([FromBody] StartTripRequest request)
        {
            try
            {
                // Validate coordinates
                if (request.StartLatitude < -90 || request.StartLatitude > 90 ||
                    request.StartLongitude < -180 || request.StartLongitude > 180 ||
                    request.EndLatitude < -90 || request.EndLatitude > 90 ||
                    request.EndLongitude < -180 || request.EndLongitude > 180)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid coordinates",
                        Details = "Latitude must be between -90 and 90, Longitude between -180 and 180"
                    });
                }

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
                    ErrorCode = "TRIP_ERROR",
                    Details = _env.IsDevelopment() ? ex.Message : null,
                    StackTrace = _env.IsDevelopment() ? ex.StackTrace : null
                });
            }
        }

        [HttpGet("{tripId}/hazards")]
        public async Task<IActionResult> GetUpcomingHazards(
            int tripId,
            [FromQuery] double currentLat,
            [FromQuery] double currentLng)
        {
            try
            {
                // Validate coordinates
                if (currentLat < -90 || currentLat > 90 || currentLng < -180 || currentLng > 180)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid coordinates",
                        Details = "Latitude must be between -90 and 90, Longitude between -180 and 180"
                    });
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                    return Unauthorized(new ErrorResponse { Message = "User not found" });

                bool hasActiveSubscription = await _tripService.HasActiveSubscription(userId);

                if (user.FreeTripsUsed >= TripService.FREE_TRIPS_LIMIT && !hasActiveSubscription)
                {
                    return Unauthorized(new ErrorResponse
                    {
                        Message = "Hazard notifications require a subscription",
                        ErrorCode = "REQUIRES_SUBSCRIPTION",
                        ActionRequired = "subscribe",
                        SubscribeUrl = "/api/subscription/plans"
                    });
                }

                var hazards = await _tripService.GetUpcomingHazards(tripId, currentLat, currentLng);
                return Ok(hazards);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hazards");
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while getting hazards",
                    Details = _env.IsDevelopment() ? ex.Message : null,
                    StackTrace = _env.IsDevelopment() ? ex.StackTrace : null
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

        [HttpPost("abort/{tripId}")]
        public async Task<IActionResult> AbortTrip(int tripId)
        {
            try
            {
                var success = await _tripService.AbortTripAsync(tripId);
                return Ok(new { Success = success });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{tripId}")]
        public async Task<IActionResult> DeleteTrip(int tripId)
        {
            try
            {
                var success = await _tripService.DeleteTripAsync(tripId);
                return Ok(new { Success = success });
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

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
            _logger.LogError(ex, "Error in TripController");

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
                    ErrorCode = "INTERNAL_ERROR",
                    Details = _env.IsDevelopment() ? ex.Message : null,
                    StackTrace = _env.IsDevelopment() ? ex.StackTrace : null
                })
            };
        }
    }
}
namespace PATHLY_API.Dto
{
    public class ErrorResponse
    {
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string ActionRequired { get; set; }
        public string SubscribeUrl { get; set; }

        public string Details { get; set; }
        public string StackTrace { get; set; }
    }
}
