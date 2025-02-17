using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{
	public class TokenRequestModel
	{
		[Required]
		public string Email { get; set; }

		[Required]
		public string Password { get; set; }
	}
}
