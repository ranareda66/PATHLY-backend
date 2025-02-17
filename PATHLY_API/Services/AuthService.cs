using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PATHLY_API.JWT;
using PATHLY_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PATHLY_API.Services
{
	public class AuthService : IAuthService
	{
		private readonly UserManager<User> _userManager;
		private readonly jwt _jwt;
		public AuthService(UserManager<User> userManager, IOptions<jwt> jwt)
        {
			_userManager = userManager;
			_jwt = jwt.Value;
		}
		public async Task<AuthModel> RegisterAsync(RegisterModel model)
		{
			if (await _userManager.FindByEmailAsync(model.Email) is not null)
				return new AuthModel { Message = "Email is already registered!" };

			if (await _userManager.FindByNameAsync(model.Username) is not null)
				return new AuthModel { Message = "Username is already registered!" };

			User user;
			if (model.IsAdmin)
			{
				user = new Admin
				{
					UserName = model.Username,
					Email = model.Email,
				};
			}
			else
			{
				user = new User
				{
					UserName = model.Username,
					Email = model.Email,
				};
			}

			var result = await _userManager.CreateAsync(user, model.Password);

			if (!result.Succeeded)
			{
				var errors = string.Empty;

				foreach (var error in result.Errors)
					errors += $"{error.Description},";

				return new AuthModel { Message = errors };
			}
			var jwtSecurityToken = await CreateJwtToken(user);

			return new AuthModel
			{
				Email = user.Email,
				ExpiresOn = jwtSecurityToken.ValidTo,
				IsAuthenticated = true,
				Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
				Username = user.UserName
			};
		}

		public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
		{
			var authModel = new AuthModel();

			var user = await _userManager.FindByEmailAsync(model.Email);

			if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
			{
				authModel.Message = "Email or Password is incorrect!";
				return authModel;
			}

			var jwtSecurityToken = await CreateJwtToken(user);

			authModel.IsAuthenticated = true;
			authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
			authModel.Email = user.Email;
			authModel.Username = user.UserName;
			authModel.ExpiresOn = jwtSecurityToken.ValidTo;

			return authModel;
		}

		private async Task<JwtSecurityToken> CreateJwtToken(User user)
		{
			var userClaims = await _userManager.GetClaimsAsync(user);

			var claims = new[]
			{
			   new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
			   new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			   new Claim(JwtRegisteredClaimNames.Email, user.Email),
			   new Claim("id", user.Id.ToString()),
			   new Claim("isAdmin", (user is Admin).ToString()) // Add a claim to distinguish between User and Admin
			}
			.Union(userClaims);

			var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
			var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

			var jwtSecurityToken = new JwtSecurityToken(
				issuer: _jwt.Issuer,
				audience: _jwt.Audience,
				claims: claims,
				expires: DateTime.Now.AddDays(_jwt.ExpiresAt),
				signingCredentials: signingCredentials);

			return jwtSecurityToken;
		}
	}
}
