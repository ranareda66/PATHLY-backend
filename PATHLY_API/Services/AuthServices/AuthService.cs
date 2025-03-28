using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PATHLY_API.Data;
using PATHLY_API.JWT;
using PATHLY_API.Models;
using PATHLY_API.Services.EmailServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PATHLY_API.Services.AuthServices
{
    public class AuthService : IAuthService 
    {
        private readonly UserManager<User> _userManager;
        private readonly Jwt _jwt;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;

        public AuthService(UserManager<User> userManager, IOptions<Jwt> jwt, IEmailService emailService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _emailService = emailService;
            _context = context;
        }

        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email is already registered!" };

            if (await _userManager.FindByNameAsync(model.Username) is not null)
                return new AuthModel { Message = "Username is already registered!" };


            User user = model.IsAdmin ? new Admin
            {
                UserName = model.Username,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Email = model.Email
            }
            : new User
            {
                UserName = model.Username,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Email = model.Email
            };


            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthModel { Message = errors };
            }

            var jwtSecurityToken = await CreateJwtToken(user);

            var refreshToken = GenerateRefreshToken();
            user.RefreshTokens?.Add(refreshToken);
            await _userManager.UpdateAsync(user);


            return new AuthModel
            {
                Email = user.Email,
                ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Username = user.UserName,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.ExpiresOn
            };
        }
        public async Task<AuthModel> LoginAsync(LoginModel model)
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

            if (user.RefreshTokens.Any(t => t.IsActive))
            {
                var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
                authModel.RefreshToken = activeRefreshToken.Token;
                authModel.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
            }
            else
            {
                var refreshToken = GenerateRefreshToken();
                authModel.RefreshToken = refreshToken.Token;
                authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
                user.RefreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);
            }

            return authModel;
        }
        private async Task<JwtSecurityToken> CreateJwtToken(User user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);

            var claims = new List<Claim>
            {
               new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
               new Claim(JwtRegisteredClaimNames.Email, user.Email),
               new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
               new Claim("isAdmin", (user is Admin).ToString())
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
        public async Task<AuthModel> RefreshTokenAsync(string token)
        {
            var authModel = new AuthModel();

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
                return new AuthModel { Message = "Invalid token." };

            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

            if (!refreshToken.IsActive)
                return new AuthModel { Message = "Inactive token." };


            refreshToken.RevokedOn = DateTime.UtcNow;

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);

            var jwtToken = await CreateJwtToken(user);
            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            authModel.Email = user.Email;
            authModel.Username = user.UserName;
            authModel.RefreshToken = newRefreshToken.Token;
            authModel.RefreshTokenExpiration = newRefreshToken.ExpiresOn;

            return authModel;
        }
        public async Task<bool> RevokeTokenAsync(string token)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
                return false;

            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

            if (!refreshToken.IsActive)
                return false;

            refreshToken.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return true;
        }
        public async Task<string> SendPasswordResetCodeAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return "User not found.";

           
            var code = new Random().Next(100000, 999999).ToString();

           
            var resetCode = new PasswordResetCode
            {
                Email = email,
                Code = code,
                ExpirationTime = DateTime.UtcNow.AddMinutes(10) 
            };

            _context.PasswordResetCodes.Add(resetCode);
            await _context.SaveChangesAsync();

            
            await _emailService.SendEmailAsync(email, "Password Reset Code", $"Your password reset code is: {code}");

            return "Password reset code has been sent to your email.";
        }
        public async Task<string> ResetPasswordWithCodeAsync(string email, string code, string newPassword, string confirmPassword)
        {



            if (newPassword != confirmPassword)
                return "The new password and confirmation password do not match.";

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return "User not found.";

            var resetCode = await _context.PasswordResetCodes
                .FirstOrDefaultAsync(rc => rc.Email == email && rc.Code == code);

            if (resetCode == null || resetCode.ExpirationTime < DateTime.UtcNow)
                return "Invalid or expired code.";

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
                return "Failed to reset password.";

            _context.PasswordResetCodes.Remove(resetCode);
            user.RefreshTokens?.Clear();
            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            return "Password has been reset successfully.";

        }
        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            RandomNumberGenerator.Fill(randomNumber);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddDays(10),
                CreatedOn = DateTime.UtcNow
            };
        }
    }
}
