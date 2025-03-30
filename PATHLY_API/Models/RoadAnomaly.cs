using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using PATHLY_API.Models;
using PATHLY_API.Models.Enums;

public class RoadAnomaly
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public ProblemType Type { get; set; } 
    public string Severity { get; set; }

    [Required , ForeignKey("Road")]
    public int RoadId { get; set; }

    public virtual Road Road { get; set; }
    public virtual Location Location { get; set; }
}
