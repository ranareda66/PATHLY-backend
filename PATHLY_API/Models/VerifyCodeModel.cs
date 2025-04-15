using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{
    public class VerifyCodeModel
    {
        [Required , EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
