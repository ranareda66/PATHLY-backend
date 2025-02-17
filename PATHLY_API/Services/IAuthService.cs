using PATHLY_API.Models;

namespace PATHLY_API.Services
{
	public interface IAuthService
	{
		Task<AuthModel> RegisterAsync(RegisterModel model);
		Task<AuthModel> GetTokenAsync(TokenRequestModel model);
	}
}
