using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{
  
    public class ForgotPasswordModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } 
    }
}

