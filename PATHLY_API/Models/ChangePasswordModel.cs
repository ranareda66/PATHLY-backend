using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{
    public class ChangePasswordModel
    {

        [Required]
        public string Password { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
