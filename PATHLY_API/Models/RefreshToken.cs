using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{

    [Owned]
    public class RefreshToken
    {
        [Required]
        public string Token { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ExpiresOn { get; set; }
        public DateTime? RevokedOn { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
        public bool IsActive => RevokedOn == null && !IsExpired;

    }
}