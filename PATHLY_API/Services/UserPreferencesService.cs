using PATHLY_API.Data;

namespace PATHLY_API.Services
{
	public class UserPreferencesService
	{
		private readonly ApplicationDbContext _context;

		public UserPreferencesService(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task UpdatePreferencesAsync(int userId, bool shortestPath, bool bestROI, bool mostUsed)
		{
			var userPreferences = await _context.UserPreferences.FindAsync(userId);
			if (userPreferences == null)
			{
				throw new Exception("UserPreferences not found");
			}

			// Perform business logic
			userPreferences.ShortestPath = shortestPath;
			userPreferences.BestROI = bestROI;
			userPreferences.MostUsed = mostUsed;
			userPreferences.LastUpdated = DateTime.Now;

			await _context.SaveChangesAsync();
		}
	}
}
