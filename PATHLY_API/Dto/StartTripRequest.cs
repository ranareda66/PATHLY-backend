using System.ComponentModel.DataAnnotations;

public class StartTripRequest
{
    [Required]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double StartLatitude { get; set; }

    [Required]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double StartLongitude { get; set; }

    [Required]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public double EndLatitude { get; set; }

    [Required]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public double EndLongitude { get; set; }

    public bool RequestRoadPrediction { get; set; } = true;
}
