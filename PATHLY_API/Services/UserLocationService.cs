using PATHLY_API.Data;

namespace PATHLY_API.Services
{
	public class UserLocationService
	{
		private readonly ApplicationDbContext _context;
		public UserLocationService(ApplicationDbContext context)
		{
			_context = context;
		}
		public async Task UpdateLocationAsync(int userLocationId, decimal latitude, decimal longitude)
		{
			var userLocation = await _context.UserLocations.FindAsync(userLocationId);
			if (userLocation == null)
			{
				throw new Exception("User location not found");
			}

			// Perform business logic
			userLocation.Latitude = latitude;
			userLocation.Longitude = longitude;
			userLocation.Timestamp = DateTime.Now;

			await _context.SaveChangesAsync();
		}
	}
}
