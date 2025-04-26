//using Microsoft.AspNetCore.Mvc;
//using PATHLY_API.Services;
//using PATHLY_API.Dto;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;

//namespace PATHLY_API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    [Authorize]
//    public class TripController : ControllerBase
//    {
//        private readonly TripService _tripService;

//        public TripController(TripService tripService)
//        {
//            _tripService = tripService;
//        }

//        /// <summary>
//        /// Starts a new trip with coordinates
//        /// </summary>
//        /// <param name="request">Trip coordinates and options</param>
//        /// <returns>Trip details with route information</returns>
//        [HttpPost("start-with-coordinates")]
//        public async Task<IActionResult> StartTripWithCoordinates(
//            [FromBody] StartTripWithCoordinatesRequestDto request)
//        {
//            try
//            {
//                var result = await _tripService.StartTripWithCoordinatesAsync(
//                    request.StartLatitude,
//                    request.StartLongitude,
//                    request.EndLatitude,
//                    request.EndLongitude,
//                    request.RequestRoadPrediction);

//                return Ok(result);
//            }
//            catch (Exception ex)
//            {
//                return HandleException(ex);
//            }
//        }

//        /// <summary>
//        /// Ends an ongoing trip
//        /// </summary>
//        /// <param name="tripId">ID of the trip to end</param>
//        /// <param name="feedback">Optional feedback rating (1-5)</param>
//        [HttpPost("end/{tripId}")]
//        public async Task<IActionResult> EndTrip(int tripId, [FromQuery] int? feedback = null)
//        {
//            try
//            {
//                var success = await _tripService.EndTripAsync(tripId, feedback);
//                return Ok(new { Success = success, Message = "Trip ended successfully" });
//            }
//            catch (Exception ex)
//            {
//                return HandleException(ex);
//            }
//        }

//        /// <summary>
//        /// Aborts an ongoing trip
//        /// </summary>
//        /// <param name="tripId">ID of the trip to abort</param>
//        [HttpPost("abort/{tripId}")]
//        public async Task<IActionResult> AbortTrip(int tripId)
//        {
//            try
//            {
//                var success = await _tripService.AbortTripAsync(tripId);
//                return Ok(new { Success = success, Message = "Trip aborted successfully" });
//            }
//            catch (Exception ex)
//            {
//                return HandleException(ex);
//            }
//        }

//        /// <summary>
//        /// Deletes a cancelled trip
//        /// </summary>
//        /// <param name="tripId">ID of the trip to delete</param>
//        [HttpDelete("{tripId}")]
//        public async Task<IActionResult> DeleteTrip(int tripId)
//        {
//            try
//            {
//                var success = await _tripService.DeleteTripAsync(tripId);
//                return Ok(new { Success = success, Message = "Trip deleted successfully" });
//            }
//            catch (Exception ex)
//            {
//                return HandleException(ex);
//            }
//        }

//        /// <summary>
//        /// Gets trip history for the authenticated user
//        /// </summary>
//        /// <param name="startDate">Optional filter by start date</param>
//        /// <param name="status">Optional filter by status</param>
//        [HttpGet("history")]
//        public async Task<IActionResult> GetTripHistory(
//            [FromQuery] DateTime? startDate = null,
//            [FromQuery] TripStatus? status = null)
//        {
//            try
//            {
//                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
//                var trips = await _tripService.GetUserTripsAsync(userId, startDate, status);
//                return Ok(trips);
//            }
//            catch (Exception ex)
//            {
//                return HandleException(ex);
//            }
//        }

//        /// <summary>
//        /// Gets details for a specific trip
//        /// </summary>
//        /// <param name="tripId">ID of the trip to retrieve</param>
//        [HttpGet("{tripId}")]
//        public async Task<IActionResult> GetTripDetails(int tripId)
//        {
//            try
//            {
//                var trip = await _tripService.GetTripDetailsAsync(tripId);
//                return Ok(trip);
//            }
//            catch (Exception ex)
//            {
//                return HandleException(ex);
//            }
//        }

//        private IActionResult HandleException(Exception ex)
//        {
//            return ex switch
//            {
//                UnauthorizedAccessException => Unauthorized(new
//                {
//                    Message = ex.Message,
//                    StatusCode = 401
//                }),
//                KeyNotFoundException => NotFound(new
//                {
//                    Message = ex.Message,
//                    StatusCode = 404
//                }),
//                ArgumentException => BadRequest(new
//                {
//                    Message = ex.Message,
//                    StatusCode = 400
//                }),
//                InvalidOperationException => BadRequest(new
//                {
//                    Message = ex.Message,
//                    StatusCode = 400
//                }),
//                _ => StatusCode(500, new
//                {
//                    Message = "An error occurred while processing your request",
//                    StatusCode = 500
//                })
//            };
//        }
//    }
//}
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