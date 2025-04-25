using PATHLY_API.Models;

namespace PATHLY_API.Services
{
	public interface IRoadPredictionService
	{
		Task<List<ClusterGroup>> PredictRoadConditionAsync(double startLat, double startLng, double endLat, double endLng);
	}
}
