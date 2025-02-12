using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
    public class QualityMetric
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public decimal SpeedBumpRate { get; set; }
        [Required]
        public decimal PotholeRate { get; set; }
        [Required]
        public decimal RoughnessRate { get; set; }
        [Required]
        public decimal GoodRoadRate { get; set; }

        [Required, ForeignKey("Road")]
        public int RoadId { get; set; }

        public Road Road { get; set; }
    }
}
