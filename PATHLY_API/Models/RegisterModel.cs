using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{
    public class RegisterModel
    {

        [StringLength(100)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string LastName { get; set; }

        [Required, StringLength(20)]
        public string Username { get; set; }

        [Required, StringLength(50)]
        public string Email { get; set; }

        [Required, StringLength(50)]
        public string Password { get; set; }

        [DefaultValue(false)]
        public bool IsAdmin { get; set; } = false;
    }
}
