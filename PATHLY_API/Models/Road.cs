using PATHLY_API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Road
{
    [Key]
    public int RoadId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty; // ✅ Default value

    [Range(-90, 90)]
    public decimal Latitude { get; set; }

    [Range(-180, 180)]
    public decimal Longitude { get; set; }

    public string Conditions { get; set; } = string.Empty; //  Default value
	public string Region { get; set; } = string.Empty; //  Default value

    public decimal Length { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime LastUpdate { get; set; } = DateTime.Now;

    // Make Navigation Properties Nullable
    public ICollection<RoadAnomalies>? RoadAnomalies { get; set; } = new List<RoadAnomalies>();
    public ICollection<RoadRecommendation>? RoadRecommendations { get; set; } = new List<RoadRecommendation>();
}
