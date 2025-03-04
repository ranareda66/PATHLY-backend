using PATHLY_API.Models;

namespace PATHLY_API.Services.AuthServices
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterModel model);
        Task<AuthModel> GetTokenAsync(TokenRequestModel model);
        Task<AuthModel> RefreshTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);
        Task<string> SendPasswordResetCodeAsync(string email);
        Task<string> ResetPasswordWithCodeAsync(string email, string code, string newPassword, string confirmPassword);
    }
}
