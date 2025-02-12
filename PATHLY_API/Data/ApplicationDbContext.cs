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
        public DbSet<Road> Roads { get; set; }
        public DbSet<Trip> Trips  { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<RoadAnomalies> RoadAnomalies { get; set; }
        public DbSet<QualityMetric> QualityMetrics { get; set; }
        public DbSet<Search> Searchs { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            optionsBuilder.UseSqlServer
                 (@"Data Source=AHMED\SQLEXPRESS;Initial Catalog=PATHLY;Trusted_Connection=True;Integrated Security=True;Trust Server Certificate=False;");

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

            // Ensuring a unique combination of UserId and SubscriptionPlanId
            modelBuilder.Entity<UserSubscription>()
            .HasIndex(us => new { us.UserId, us.SubscriptionPlanId })
            .IsUnique(); // This ensures a user cannot have multiple subscriptions for the same plan

            // Road - RoadAnomalies Relationship
            modelBuilder.Entity<Road>()
                .HasMany(r => r.RoadAnomalies)
                .WithOne(a => a.Road)
                .HasForeignKey(a => a.RoadId);

            // User - SearchHistory Relationship
            modelBuilder.Entity<Search>()
                .HasOne(sh => sh.User)
                .WithMany()
                .HasForeignKey(sh => sh.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Trip - Road Relationship
            modelBuilder.Entity<Trip>()
                .HasMany(t => t.Roads) 
                .WithOne(r => r.Trip) 
                .HasForeignKey(r => r.TripId) 
                .OnDelete(DeleteBehavior.Cascade);

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
