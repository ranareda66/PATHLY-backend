using PATHLY_API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using PATHLY_API.Models.Enums;

public class Road
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string Start { get; set; }
    [Required]
    public string Destination { get; set; }

    [Required]
    public decimal Length { get; set; }
    [Required]
    public decimal QualityScore { get; set; }

    public RoadQuality Quality { get; set; }

    public DateTime LastUpdate { get; internal set; } = DateTime.UtcNow;


    [Required , ForeignKey("Trip")]
    public int TripId { get; set; } 

    public Trip Trip { get; set; }
    public QualityMetric QualityMetric { get; set; }
    public ICollection<RoadAnomalies> RoadAnomalies { get; set; }

    public ICollection<TripRoad> TripRoads { get; set; }

}
