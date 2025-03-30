using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
    public class PasswordResetCode
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(6, MinimumLength = 6)]
        public string Code { get; set; }

        public DateTime ExpirationTime { get; set; } = DateTime.UtcNow.AddMinutes(15);
    }
}
