using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{
    public class ChangeEmailModel
    {
        public string? UserId { get; set; }

        [Required, EmailAddress]
        public string NewEmail { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
