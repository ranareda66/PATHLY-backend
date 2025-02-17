using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PATHLY_API.Models;
using Microsoft.AspNetCore.Identity;

namespace PATHLY_API.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
	{
        public ApplicationDbContext()
        {
        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Road> Roads { get; set; }
        public DbSet<Trip> Trips  { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Search> Searchs { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<RoadAnomalies> RoadAnomalies { get; set; }
        public DbSet<QualityMetric> QualityMetrics { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            optionsBuilder.UseSqlServer
                 ("Server=.;Database=PATHLY;Trusted_Connection=True;Trust Server Certificate=true");

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
			modelBuilder.Entity<Admin>().ToTable("Admins");

			// Ensure Identity tables are properly configured to use int keys
			modelBuilder.Entity<IdentityUserLogin<int>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });
			modelBuilder.Entity<IdentityUserRole<int>>().HasKey(r => new { r.UserId, r.RoleId });
			modelBuilder.Entity<IdentityUserToken<int>>().HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

			// Create Composite Key For TripRoad Class
			modelBuilder.Entity<TripRoad>()
                .HasKey(tr => new { tr.RoadId, tr.TripId });

            // Ensuring a unique combination of UserId and SubscriptionPlanId
            modelBuilder.Entity<UserSubscription>()
            .HasIndex(us => new { us.UserId, us.SubscriptionPlanId })
            .IsUnique(); // This ensures a user cannot have multiple subscriptions for the same plan

            // User - User Subscription Relationship
            modelBuilder.Entity<UserSubscription>()
            .HasOne(us => us.User)
            .WithMany()
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);


            // User Subscription - Subscription Plan Relationship
            modelBuilder.Entity<UserSubscription>()
                .HasOne(us => us.SubscriptionPlan)
                .WithMany()
                .HasForeignKey(us => us.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Road - RoadAnomalies Relationship
            modelBuilder.Entity<Road>()
                .HasMany(r => r.RoadAnomalies)
                .WithOne(a => a.Road)
                .HasForeignKey(a => a.RoadId)
                .OnDelete(DeleteBehavior.Restrict);


            // User - Search Relationship
            modelBuilder.Entity<Search>()
                .HasOne(sh => sh.User)
                .WithMany()
                .HasForeignKey(sh => sh.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // Trip - TripRoad Relationship
            modelBuilder.Entity<Trip>()
                .HasMany(t => t.TripRoads)
                .WithOne(r => r.Trip)
                .HasForeignKey(r => r.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            // Road - TripRoad Relationship
            modelBuilder.Entity<Road>()
                .HasMany(t => t.TripRoads)
                .WithOne(r => r.Road)
                .HasForeignKey(r => r.RoadId)
                .OnDelete(DeleteBehavior.Restrict);


            // Report - Attachment Relationship
            modelBuilder.Entity<Report>()
                .HasMany(r => r.Attachments)
                .WithOne(a => a.Report)
                .HasForeignKey(a => a.ReportId)
                .OnDelete(DeleteBehavior.Cascade);


            // User - Report Relationship
            modelBuilder.Entity<User>()
                 .HasMany(u => u.Reports)
                 .WithOne(r => r.User)
                 .HasForeignKey(r => r.UserId)
                 .OnDelete(DeleteBehavior.Restrict);


            // Road - QualityMetric Relationship
            modelBuilder.Entity<Road>()
                .HasOne(r => r.QualityMetric)
                .WithOne(q => q.Road)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
