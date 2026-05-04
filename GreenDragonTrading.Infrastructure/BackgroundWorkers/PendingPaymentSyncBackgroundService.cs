using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    /// <summary>
    /// Periodically syncs pending payment orders that have reached their expiration time.
    /// Uses a Redis sorted set where score = expiration Unix timestamp.
    /// </summary>
    public class PendingPaymentSyncBackgroundService : BackgroundService
    {
        private static readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(5);

        private readonly ILogger<PendingPaymentSyncBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public PendingPaymentSyncBackgroundService(
            ILogger<PendingPaymentSyncBackgroundService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Pending payment sync background service started.");

            await RunSyncCycleAsync(stoppingToken);

            using var timer = new PeriodicTimer(SyncInterval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await RunSyncCycleAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while syncing pending payments.");
                }
            }

            _logger.LogInformation("Pending payment sync background service stopped.");
        }

        private async Task RunSyncCycleAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var redis = scope.ServiceProvider.GetRequiredService<IRedisService>();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

            var queueKey = RedisConstants.PendingPaymentSyncQueue();
            var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Get only due items (score <= now), already sorted by nearest expiration first.
            var dueOrderCodes = await redis.SortedSetRangeByScoreAsync(
                queueKey,
                double.NegativeInfinity,
                nowUnix);

            if (dueOrderCodes.Count == 0)
            {
                return;
            }

            var syncedCount = 0;
            var invalidCount = 0;

            foreach (var member in dueOrderCodes)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (!long.TryParse(member, out var orderCode))
                {
                    invalidCount++;
                    await redis.SortedSetRemoveAsync(queueKey, member);

                    _logger.LogWarning(
                        "Removed invalid member from payment sync queue: Member={Member}",
                        member);
                    continue;
                }

                try
                {
                    await paymentService.SyncPaymentAsync(orderCode, cancellationToken);
                    syncedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed syncing pending payment OrderCode={OrderCode}", orderCode);
                }
            }

            _logger.LogInformation(
                "Pending payment sync cycle completed. Due={DueCount}, Synced={SyncedCount}, Invalid={InvalidCount}",
                dueOrderCodes.Count,
                syncedCount,
                invalidCount);
        }
    }
}
