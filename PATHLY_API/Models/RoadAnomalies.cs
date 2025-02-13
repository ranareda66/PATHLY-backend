using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class RoadAnomalies
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string Type { get; set; } 
    public string Severity { get; set; }

    public string Location { get; set; } 

    [Required , ForeignKey("Road")]
    public int RoadId { get; set; } 

    public Road Road { get; set; }
}
