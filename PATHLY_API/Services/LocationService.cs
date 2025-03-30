using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Models;

namespace PATHLY_API.Services
{
	public class LocationService
	{
		private readonly ApplicationDbContext _context;
		public LocationService(ApplicationDbContext context) => _context = context;


        public async Task TrackUserLocationAsync(int userId, decimal latitude, decimal longitude)
        {
            var user = await _context.Users.FirstOrDefaultAsync(a => a.Id == userId);
            if (user is null)
                throw new Exception("User not found");

            var lastLocation = await _context.Locations
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.UpdatedAt)
                .FirstOrDefaultAsync();

            if (lastLocation?.Latitude == latitude && lastLocation?.Longitude == longitude)
                return; // No need to add a new record if the location has not changed.


            var newlocation = new Location
            {
                UserId = userId,
                Latitude = latitude,
                Longitude = longitude,
            };

            await _context.Locations.AddAsync(newlocation);
            await _context.SaveChangesAsync();
        }

        public async Task<Location> GetLatestUserLocationAsync(int userId)
        {
            var location = await _context.Locations
                .Where(ulh => ulh.UserId == userId)
                .OrderByDescending(ulh => ulh.UpdatedAt)
                .FirstOrDefaultAsync();

            if (location is null)
                throw new InvalidOperationException($"No location found for user with ID {userId}.");

            return location;
        }
    }
}
