using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{
    public class RevokeToken
    {
        [Required]
        public string Token { get; set; }
    }
}