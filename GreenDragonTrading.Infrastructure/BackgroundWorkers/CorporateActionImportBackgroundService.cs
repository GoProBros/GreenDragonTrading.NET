using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportCorporateActions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    /// <summary>
    /// Imports upcoming corporate actions from DNSE daily at 22:00 (GMT+7).
    /// </summary>
    public class CorporateActionImportBackgroundService : BackgroundService
    {
        private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

        private readonly ILogger<CorporateActionImportBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public CorporateActionImportBackgroundService(
            ILogger<CorporateActionImportBackgroundService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Corporate action import background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var delay = GetDelayUntilNextRun(DateTimeOffset.UtcNow);
                    _logger.LogInformation("Next corporate action import scheduled after {Delay}.", delay);

                    await Task.Delay(delay, stoppingToken);
                    await RunImportCycleSafeAsync("daily-22:00", stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in corporate action import schedule loop.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Corporate action import background service stopped.");
        }

        private static TimeSpan GetDelayUntilNextRun(DateTimeOffset utcNow)
        {
            var vnNow = utcNow.ToOffset(VnOffset);
            var nextRun = new DateTimeOffset(
                vnNow.Year,
                vnNow.Month,
                vnNow.Day,
                22,
                0,
                0,
                vnNow.Offset);

            if (vnNow >= nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            var delay = nextRun - vnNow;
            return delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
        }

        private async Task RunImportCycleSafeAsync(string trigger, CancellationToken cancellationToken)
        {
            try
            {
                await RunImportCycleAsync(trigger, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during corporate action import cycle. Trigger={Trigger}", trigger);
            }
        }

        private async Task RunImportCycleAsync(string trigger, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            _logger.LogInformation("Starting corporate action import cycle. Trigger={Trigger}", trigger);

            var result = await mediator.Send(new ImportCorporateActionsUpcomingCommand(), cancellationToken);

            _logger.LogInformation(
                "Corporate action import cycle completed. Trigger={Trigger}, Fetched={FetchedCount}, Inserted={InsertedCount}, Updated={UpdatedCount}, Skipped={SkippedCount}, Message={Message}",
                trigger,
                result.Data?.FetchedCount ?? 0,
                result.Data?.InsertedCount ?? 0,
                result.Data?.UpdatedCount ?? 0,
                result.Data?.SkippedCount ?? 0,
                result.Message);
        }
    }
}
