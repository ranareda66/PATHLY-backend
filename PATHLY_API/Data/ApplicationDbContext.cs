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
		public DbSet<UserSubscription> UserSubscriptions { get; set; }
	    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer("Server=.;Database=PATHLY;Trusted_Connection=True;Trust Server Certificate=true");
			base.OnConfiguring(optionsBuilder);
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<UserSubscription>()
			.HasOne(us => us.User) 
			.WithMany() 
			.HasForeignKey(us => us.UserId) 
			.OnDelete(DeleteBehavior.Cascade); 

			modelBuilder.Entity<UserSubscription>()
				.HasOne(us => us.SubscriptionPlan) 
				.WithMany() 
				.HasForeignKey(us => us.SubscriptionPlanId) 
				.OnDelete(DeleteBehavior.Restrict);

			// Ensuring a unique combination of UserId and SubscriptionPlanId
			modelBuilder.Entity<UserSubscription>()
			.HasIndex(us => new { us.UserId, us.SubscriptionPlanId })
			.IsUnique(); // This ensures a user cannot have multiple subscriptions for the same plan
		}
	}
}
