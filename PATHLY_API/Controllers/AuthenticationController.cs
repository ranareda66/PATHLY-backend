using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Models;
using PATHLY_API.Services.AuthServices;

namespace PATHLY_API.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthenticationController(IAuthService authService) => _authService = authService;

		[HttpPost("register")]
		public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _authService.RegisterAsync(model);


			if (!result.IsAuthenticated)
				return BadRequest(result.Message);

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
		}

		[HttpPost("getToken")]
		public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequestModel model)
		{
			var result = await _authService.GetTokenAsync(model);

			if (!result.IsAuthenticated)
				return BadRequest(result.Message);

            if (!string.IsNullOrEmpty(result.RefreshToken))
                SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
		}

        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var result = await _authService.RefreshTokenAsync(refreshToken);

            if (!result.IsAuthenticated)
                return BadRequest(result);

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }

        [HttpPost("revokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeToken model)
        {
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required!");

            var result = await _authService.RevokeTokenAsync(token);

            if (!result)
                return BadRequest("Token is invalid!");

            return Ok();
        }


        private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires.ToLocalTime(),
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
