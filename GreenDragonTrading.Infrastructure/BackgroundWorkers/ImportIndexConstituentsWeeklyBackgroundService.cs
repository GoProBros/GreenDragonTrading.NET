using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportIndexConstituentsFromSsi;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    /// <summary>
    /// Imports index constituents weekly on Sunday at 03:00 (GMT+7).
    /// </summary>
    public class ImportIndexConstituentsWeeklyBackgroundService : BackgroundService
    {
        private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

        private readonly ILogger<ImportIndexConstituentsWeeklyBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public ImportIndexConstituentsWeeklyBackgroundService(
            ILogger<ImportIndexConstituentsWeeklyBackgroundService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Weekly index constituents import background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var delay = GetDelayUntilNextRun(DateTimeOffset.UtcNow);
                    _logger.LogInformation("Next index constituents import scheduled after {Delay}.", delay);

                    await Task.Delay(delay, stoppingToken);
                    await RunImportCycleSafeAsync("weekly-sun-03:00", stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in index constituents import schedule loop.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Weekly index constituents import background service stopped.");
        }

        private static TimeSpan GetDelayUntilNextRun(DateTimeOffset utcNow)
        {
            var vnNow = utcNow.ToOffset(VnOffset);
            var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)vnNow.DayOfWeek + 7) % 7;

            var nextRun = new DateTimeOffset(
                vnNow.Year,
                vnNow.Month,
                vnNow.Day,
                3,
                0,
                0,
                vnNow.Offset).AddDays(daysUntilSunday);

            if (vnNow >= nextRun)
            {
                nextRun = nextRun.AddDays(7);
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
                _logger.LogError(ex, "Unexpected error during index constituents import cycle. Trigger={Trigger}", trigger);
            }
        }

        private async Task RunImportCycleAsync(string trigger, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            _logger.LogInformation("Starting index constituents import cycle. Trigger={Trigger}", trigger);

            var result = await mediator.Send(new ImportIndexConstituentsFromSsiCommand(), cancellationToken);

            _logger.LogInformation(
                "Index constituents import cycle completed. Trigger={Trigger}, Indices={ProcessedIndices}, Upserted={UpsertedCount}, Message={Message}",
                trigger,
                result.ProcessedIndices,
                result.UpsertedCount,
                result.Message);
        }
    }
}
