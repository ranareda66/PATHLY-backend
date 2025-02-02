using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{
	public class UserPreferences
	{
		[Key] 
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
		public int PreferenceId { get; set; }

		[Required] 
		[ForeignKey("User")] 
		public int UserId { get; set; }

		[Required]
		public bool ShortestPath { get; set; }

		[Required]
		public bool BestROI { get; set; }

		[Required]
		public bool MostUsed { get; set; }
		[Required]
		public DateTime LastUpdated { get; set; }

		public User User { get; set; }

		public string GetPreferencesInfo()
		{
			return $"Shortest Path: {ShortestPath}, Best ROI: {BestROI}, Most Used: {MostUsed}, Last Updated: {LastUpdated}";
		}
	}
}
