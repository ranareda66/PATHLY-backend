using Microsoft.AspNetCore.Mvc;

namespace PATHLY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _searchService;
        public SearchController(SearchService searchService) => _searchService = searchService;

        [HttpGet("search")]
        public async Task<IActionResult> GetCoordinates([FromQuery] string address)
        {
            var result = await _searchService.SearchForAddress(address, User);
            if (result is null)
                return NotFound("Address not found");

            return Ok(result);
        }
    }
}
