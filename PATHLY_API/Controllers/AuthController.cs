using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Models;
using PATHLY_API.Services;

namespace PATHLY_API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _authService.RegisterAsync(model);

			if (!result.IsAuthenticated)
				return BadRequest(result.Message);

			return Ok(result);
		}

		[HttpPost("gettoken")]
		public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequestModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _authService.GetTokenAsync(model);

			if (!result.IsAuthenticated)
				return BadRequest(result.Message);

			return Ok(result);
		}
	}
}
