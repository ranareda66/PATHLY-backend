using PATHLY_API.Models;

namespace PATHLY_API.Services.AuthServices
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterModel model);
        Task<AuthModel> LoginAsync(LoginModel model);
        Task<AuthModel> RefreshTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);
        Task<string> SendPasswordResetCodeAsync(string email);
        Task<string> VerifyResetCodeAsync(string email, string code);

        Task<string> ResetPasswordWithCodeAsync(string email, string newPassword, string confirmPassword);
    }
}
