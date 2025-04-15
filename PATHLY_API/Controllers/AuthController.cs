using Microsoft.AspNetCore.Mvc;
using PATHLY_API.Models;
using PATHLY_API.Services.AuthServices;

namespace PATHLY_API.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		public AuthController(IAuthService authService) => _authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _authService.RegisterAsync(model);


            if (!result.IsAuthenticated)
                return Unauthorized(result.Message);

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(new
            {
                result.IsAuthenticated,
                result.Token,
                result.ExpiresOn
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var result = await _authService.LoginAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            if (!string.IsNullOrEmpty(result.RefreshToken))
                SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(new
            {
                result.IsAuthenticated,
                result.Token,
                result.ExpiresOn
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshToken model = null)
        {
            var token = model?.Token ?? Request.Cookies["refresh-token"];

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required.");

            var result = await _authService.RefreshTokenAsync(token);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(new
            {
                NewToken = result.RefreshToken,
                NewTokenExpiration = result.RefreshTokenExpiration
            });
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeToken model)
        {
            var token = model.Token ?? Request.Cookies["refresh-token"];

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required!");

            var result = await _authService.RevokeTokenAsync(token);

            if (!result)
                return BadRequest("Token is invalid!");

            return Ok();
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> SendResetCode([FromBody] ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.SendPasswordResetCodeAsync(model.Email);

            if (result == "User not found.")
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyCodeModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.VerifyResetCodeAsync(model.Email, model.Code);

            if (result == "Incorrect or invalid Code")
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordWithCode([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ResetPasswordWithCodeAsync(model.Email, model.NewPassword, model.ConfirmPassword);

            if (result == "User not found." || result == "Invalid or expired code." || result == "Failed to reset password." || result == "The new password and confirmation password do not match.")
                return BadRequest(result);

            return Ok(result);
        }
        private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires.ToLocalTime(),
            };

            Response.Cookies.Append("refresh-token", refreshToken, cookieOptions);
        }
    }
}
