using PATHLY_API.Interfaces;
using PATHLY_API.Models;
using System.Text;
using System.Text.Json;

namespace PATHLY_API.Services
{
    public class RoadPredictionService : IRoadPredictionService
	{
		private readonly HttpClient _httpClient;
		private readonly string _aiApiUrl = "https://ai-model-production.up.railway.app/predict_in_range";

		public RoadPredictionService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<List<ClusterGroup>> PredictRoadConditionAsync(double startLat, double startLng, double endLat, double endLng)
		{
			var requestBody = new
			{
				start_lat = startLat,
				start_lon = startLng,
				end_lat = endLat,
				end_lon = endLng
			};

			var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
			var response = await _httpClient.PostAsync(_aiApiUrl, content);
			var responseString = await response.Content.ReadAsStringAsync();
			if (!response.IsSuccessStatusCode)
				throw new Exception($"Failed to fetch road condition from AI API: {response.StatusCode} - {responseString}");

			var result = JsonSerializer.Deserialize<ClusterResponse>(responseString);
			var points = result?.Predictions;

			if (points == null)
				throw new Exception("No points returned from AI API.");

			var clusterPoints = points.Select(p => new ClusterPoint
			{
				Latitude = p.Latitude,
				Longitude = p.Longitude,
				Cluster = p.Cluster
			}).ToList();


			return GroupClusters(clusterPoints);
		}

		private List<ClusterGroup> GroupClusters(List<ClusterPoint> points)
		{
			var result = new List<ClusterGroup>();
			if (points == null || points.Count == 0) return result;

			int? currentCluster = null;
			ClusterGroup currentGroup = null;

			foreach (var point in points)
			{
				if (currentCluster == null || point.Cluster != currentCluster)
				{
					// Start new group
					currentCluster = point.Cluster;
					currentGroup = new ClusterGroup
					{
						ClusterName = MapClusterToName(point.Cluster),
						Color = MapClusterToColor(point.Cluster),
						Points = new List<Coordinate>()
					};
					result.Add(currentGroup);
				}

				currentGroup.Points.Add(new Coordinate
				{
					Latitude = point.Latitude,
					Longitude = point.Longitude
				});
			}

			return result;
		}

		private string MapClusterToName(int cluster)
		{
			return cluster switch
			{
				0 => "Good Road",
				1 => "Bump",
				2 => "Hole",
				3 => "Bad Road",
				_ => "Unknown"
			};
		}
		private string MapClusterToColor(int cluster)
		{
			return cluster switch
			{
				0 => "green",
				1 => "yellow",
				2 => "orange",
				3 => "red",
				_ => "gray"
			};
		}
	}
}
