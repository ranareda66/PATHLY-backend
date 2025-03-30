using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{
    public class RegisterModel
    {
        [Required, MinLength(3), MaxLength(20)]
        [RegularExpression("^(?=.*[a-zA-Z])[a-zA-Z0-9]{3,20}$",
            ErrorMessage = "Username must be 3-20 characters long, contain only letters and numbers, and must include at least one letter.")]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MaxLength(50)]
        public string Password { get; set; }

        public bool IsAdmin { get; set; } = false;
    }
}
