using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
    public class Image
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string ImageName { get; set; }
        [Required]
        public string ImagePath { get; set; }
        [Required]
        public string ImageType { get; set; }
        [Required]
        public long ImageSize { get; set; }

        [Required, ForeignKey("Report")]
        public int ReportId { get; set; }
        public Report Report { get; set; }
    }
}
