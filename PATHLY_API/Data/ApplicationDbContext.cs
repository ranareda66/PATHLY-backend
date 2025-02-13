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
    }
}
