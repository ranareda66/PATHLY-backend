namespace PATHLY_API.Models
{
	public class ClusterGroup
	{
		public string ClusterName { get; set; } = string.Empty;
		public string Color { get; set; } = string.Empty;
		public List<Coordinate> Points { get; set; } = new();
	}
}
