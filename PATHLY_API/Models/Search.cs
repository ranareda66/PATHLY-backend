using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
    public class Search
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [Required , Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Required , Range(-180, 180)]
        public decimal Longitude { get; set; }


        [Required , ForeignKey("User")]
        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
