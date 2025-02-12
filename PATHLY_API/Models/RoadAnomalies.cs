using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class RoadAnomalies
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Type { get; set; } 
    public string Severity { get; set; }

    public string Location { get; set; } 
    public DateTime DetectionTime { get; set; } = DateTime.UtcNow;

    [Required , ForeignKey("Road")]
    public int RoadId { get; set; } 

    public Road Road { get; set; }
}
