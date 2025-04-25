using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Models;
using PATHLY_API.Services;

namespace PATHLY_API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RoadPredictionController : ControllerBase
	{
		private readonly IRoadPredictionService _roadPredictionService;

		public RoadPredictionController(IRoadPredictionService roadPredictionService)
		{
			_roadPredictionService = roadPredictionService;
		}

		[HttpPost("predict")]
		public async Task<IActionResult> PredictAsync([FromBody] LocationRequest request)
		{
			try
			{
				var result = await _roadPredictionService.PredictRoadConditionAsync(
			request.StartLatitude,
			request.StartLongitude,
			request.EndLatitude,
			request.EndLongitude
		);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error: {ex.Message}");
			}
		}

	//	[HttpGet("test-grouping")]
	//	public IActionResult TestSequentialGrouping()
	//	{
	//		var mockPoints = new List<ClusterPoint>
	//{
	//	new() { Latitude = 30.1, Longitude = 31.1, Cluster = 0 },
	//	new() { Latitude = 30.2, Longitude = 31.2, Cluster = 0 },
	//	new() { Latitude = 30.3, Longitude = 31.3, Cluster = 1 },
	//	new() { Latitude = 30.4, Longitude = 31.4, Cluster = 1 },
	//	new() { Latitude = 30.5, Longitude = 31.5, Cluster = 2 },
	//	new() { Latitude = 30.6, Longitude = 31.6, Cluster = 0 },
	//	new() { Latitude = 30.7, Longitude = 31.7, Cluster = 0 }
	//};

	//		var service = new RoadPredictionService(new HttpClient());
	//		var groups = service.GetType()
	//			.GetMethod("GroupClusters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
	//			.Invoke(service, new object[] { mockPoints });

	//		return Ok(groups);
	//	}
	}
}
