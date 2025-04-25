using System.Text.Json.Serialization;

namespace PATHLY_API.Models
{
	public class ClusterPoint
	{
		[JsonPropertyName("Latitude")]
		public double Latitude { get; set; }

		[JsonPropertyName("Longitude")]
		public double Longitude { get; set; }


		[JsonPropertyName("Predicted Cluster")]
		public int Cluster { get; set; } // 0, 1, 2, 3
	}
}
