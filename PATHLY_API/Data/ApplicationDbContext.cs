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
        public DbSet<Road> Roads { get; set; }
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<RoadAnomalies> RoadAnomalies { get; set; }
        public DbSet<RoadRecommendation> RoadRecommendations { get; set; }
        public DbSet<UserFeedback> UserFeedbacks { get; set; }

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

            // Road - RoadAnomalies Relationship
            modelBuilder.Entity<Road>()
                .HasMany(r => r.RoadAnomalies)
                .WithOne(a => a.Road)
                .HasForeignKey(a => a.RoadId);

            // Road - RoadRecommendation Relationship
            modelBuilder.Entity<Road>()
                .HasMany(r => r.RoadRecommendations)
                .WithOne(rr => rr.Road)
                .HasForeignKey(rr => rr.RoadId);

            // User - SearchHistory Relationship
            modelBuilder.Entity<SearchHistory>()
                .HasOne(sh => sh.User)
                .WithMany()
                .HasForeignKey(sh => sh.UserId);

            // User - RoadRecommendation Relationship
            modelBuilder.Entity<RoadRecommendation>()
                .HasOne(rr => rr.User)
                .WithMany()
                .HasForeignKey(rr => rr.UserId);
            //User - UserFeedback Relationship
            modelBuilder.Entity<UserFeedback>()
               .HasOne(uf => uf.User)
               .WithMany()
               .HasForeignKey(uf => uf.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
