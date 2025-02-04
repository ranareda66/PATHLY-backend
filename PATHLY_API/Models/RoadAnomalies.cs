using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class RoadAnomalies
{
    [Key]
    public int AnomalieId { get; set; }

    [Required]
    public int RoadId { get; set; } // Foreign key reference to Road

    [Required]
    public string Type { get; set; } = string.Empty; // Default value

    public string Severity { get; set; } = string.Empty; // Default value
    public string Location { get; set; } = string.Empty; // Default value
    public DateTime DetectionTime { get; set; } = DateTime.Now;

    //Make Navigation Property Nullable
    [ForeignKey("RoadId")]
    public Road? Road { get; set; }
}
