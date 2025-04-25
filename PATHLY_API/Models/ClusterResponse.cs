using System.Text.Json.Serialization;

namespace PATHLY_API.Models
{
	public class ClusterResponse
	{
		[JsonPropertyName("predictions")]
		public List<ClusterPoint> Predictions { get; set; } = new();
	}
}
