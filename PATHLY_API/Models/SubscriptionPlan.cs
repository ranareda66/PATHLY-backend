using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PATHLY_API.Models
{
	public class SubscriptionPlan
	{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

		[Required , StringLength(100)] 
		public string Name { get; set; }


        [Required , Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }


        [Required ,Range(1, int.MaxValue, ErrorMessage = "DurationInMonths must be greater than zero.")]
        public int DurationInMonths { get; set; }

		[StringLength(255)]
		public string Description { get; set; } = string.Empty;

        [JsonIgnore]
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>(); // ✅ دعم Lazy Loading



    }
}
