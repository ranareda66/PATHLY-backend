using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Models.Enums;

namespace PATHLY_API.Services
{
    public class SubscriptionExpirationService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<SubscriptionExpirationService> _logger;

        public SubscriptionExpirationService(IServiceProvider services, ILogger<SubscriptionExpirationService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Subscription Expiration Service running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var expiredSubscriptions = await dbContext.UserSubscriptions
                            .Where(us => us.Status == SubscriptionStatus.Active &&
                                        us.EndDate < DateTime.UtcNow)
                            .ToListAsync(stoppingToken);

                        foreach (var subscription in expiredSubscriptions)
                        {
                            subscription.Status = SubscriptionStatus.Expired;

                            var user = await dbContext.Users
                                .FirstOrDefaultAsync(u => u.Id == subscription.UserId, stoppingToken);

                            if (user != null)
                            {
                                var hasOtherActiveSubs = await dbContext.UserSubscriptions
                                    .AnyAsync(us => us.UserId == subscription.UserId &&
                                                  us.Status == SubscriptionStatus.Active &&
                                                  us.Id != subscription.Id,
                                            stoppingToken);

                                if (!hasOtherActiveSubs)
                                    user.SubscriptionStatus = SubscriptionStatus.Expired;
                            }
                        }

                        if (expiredSubscriptions.Count > 0)
                        {
                            await dbContext.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"Updated {expiredSubscriptions.Count} expired subscriptions.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking subscription expirations");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Runs every hour
            }
        }
    }
}