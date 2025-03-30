using PATHLY_API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{
    public class PayPalSettings
    {
        [Required]
        public string ClientId { get; set; }

        [Required]
        public string Secret { get; set; }

        [Required]
        public PayPalEnvironment Environment { get; set; }
    }
}
