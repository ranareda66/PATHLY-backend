using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Services;

[ApiController]
[Route("api/[controller]")]
public class RoadsController : ControllerBase
{
	private readonly IRoadService _roadService;

	public RoadsController(IRoadService roadService) => _roadService = roadService;

	[HttpGet("snap")]
	public async Task<IActionResult> SnapToRoads([FromQuery] string path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return BadRequest("Path is required. Format: lat1,lng1|lat2,lng2");

		try
		{
			var result = await _roadService.SnapToRoadsAsync(path);
			return Content(result, "application/json");
		}
		catch (Exception ex)
		{
			return StatusCode(500, $"An error occurred: {ex.Message}");
		}
	}
}
