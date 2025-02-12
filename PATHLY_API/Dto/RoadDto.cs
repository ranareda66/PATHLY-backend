using PATHLY_API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Dto
{
    public class RoadDto
    {
        public int Id { get; set; }
        public string Start { get; set; }
        public string Destination { get; set; }
        public decimal Length { get; set; }
        public decimal QualityScore { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
        public RoadQuality Quality { get; set; }

    }
}