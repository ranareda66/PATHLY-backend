using Microsoft.EntityFrameworkCore;
using PATHLY_API.Models;

namespace PATHLY_API.Data
{
	public class ApplicationDbContext : DbContext
	{
        public ApplicationDbContext()
        {
        }
        public ApplicationDbContext(DbContextOptions options) : base(options)
		{  
        }
		public DbSet<User> Users { get; set; }
		public DbSet<UserLocation> UserLocations { get; set; }
		public DbSet<Payment> Payments { get; set; }
		public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
		public DbSet<UserPreferences> UserPreferences { get; set; }
	    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer("Server=.;Database=PATHLY;Trusted_Connection=True;Trust Server Certificate=true");
			base.OnConfiguring(optionsBuilder);
		}
	}
}
