using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using PATHLY_API.Models;

public class RoadAnomalies
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string Type { get; set; } 
    public string Severity { get; set; }

    [Required , ForeignKey("Road")]
    public int RoadId { get; set; } 
    public Road Road { get; set; }
    public Location Location { get; set; } 
}
