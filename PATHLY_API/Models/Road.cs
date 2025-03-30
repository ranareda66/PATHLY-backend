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
    public decimal RoadLength { get; set; }

    [Required]
    public decimal QualityScore { get; set; }

    public RoadQuality Quality { get; set; }

    public DateTime LastUpdate { get; internal set; } = DateTime.UtcNow;


    public QualityMetric QualityMetric { get; set; }
    public virtual ICollection<RoadAnomaly> RoadAnomalies { get; set; } = new List<RoadAnomaly>();
    public virtual ICollection<TripRoad> TripRoads { get; set; } = new List<TripRoad>();

}
