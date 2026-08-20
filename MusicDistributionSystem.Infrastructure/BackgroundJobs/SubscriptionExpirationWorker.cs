using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicDistributionSystem.Domain.Enums;
using MusicDistributionSystem.Infrastructure.EntityFrameworkCore;

namespace MusicDistributionSystem.Infrastructure.BackgroundJobs
{
    public class SubscriptionExpirationWorker : BackgroundService
    {
        private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SubscriptionExpirationWorker> _logger;

        public SubscriptionExpirationWorker(
            IServiceProvider serviceProvider,
            ILogger<SubscriptionExpirationWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SubscriptionExpirationWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredSubscriptionsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during subscription expiration processing.");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }

            _logger.LogInformation("SubscriptionExpirationWorker stopping.");
        }

        public async Task ProcessExpiredSubscriptionsAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;

            var expiredSubscriptions = await context.UserSubscriptions
                .Include(s => s.User)
                .Where(s => s.IsActive && s.EndDateUtc <= now)
                .ToListAsync(cancellationToken);

            if (expiredSubscriptions.Count == 0) return;

            _logger.LogInformation("Found {Count} expired subscriptions to process.", expiredSubscriptions.Count);

            foreach (var sub in expiredSubscriptions)
            {
                sub.IsActive = false;

                // Check if user has any other active subscriptions
                var hasOtherActive = await context.UserSubscriptions
                    .AnyAsync(s => s.UserId == sub.UserId && s.Id != sub.Id && s.IsActive && s.EndDateUtc > now, cancellationToken);

                if (!hasOtherActive && sub.User is not null)
                {
                    sub.User.MembershipTier = MembershipTier.Free;
                    _logger.LogInformation("User {UserId} downgraded to Free tier due to subscription expiration.", sub.UserId);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
