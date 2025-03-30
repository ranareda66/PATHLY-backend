using PATHLY_API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{
    public class ReportRequestModel
    {
        [Required(ErrorMessage = "ReportType is required.")]
        public ProblemType? ReportType { get; set; }

        [Required, MaxLength(800)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Latitude is required.")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
        public decimal? Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required.")]

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
        public decimal? Longitude { get; set; } 

        [Required]
        public IFormFile Image { get; set; } 
    }

}
