using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSymbolsFromSsiV2;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    /// <summary>
    /// Imports symbols from SSI weekly on Sunday at 02:30 (GMT+7).
    /// </summary>
    public class ImportSymbolsFromSsiWeeklyBackgroundService : BackgroundService
    {
        private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

        private readonly ILogger<ImportSymbolsFromSsiWeeklyBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public ImportSymbolsFromSsiWeeklyBackgroundService(
            ILogger<ImportSymbolsFromSsiWeeklyBackgroundService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Weekly SSI symbol import background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var delay = GetDelayUntilNextRun(DateTimeOffset.UtcNow);
                    _logger.LogInformation("Next SSI symbol import scheduled after {Delay}.", delay);

                    await Task.Delay(delay, stoppingToken);
                    await RunImportCycleSafeAsync("weekly-sun-02:30", stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in SSI symbol import schedule loop.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Weekly SSI symbol import background service stopped.");
        }

        private static TimeSpan GetDelayUntilNextRun(DateTimeOffset utcNow)
        {
            var vnNow = utcNow.ToOffset(VnOffset);
            var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)vnNow.DayOfWeek + 7) % 7;

            var nextRun = new DateTimeOffset(
                vnNow.Year,
                vnNow.Month,
                vnNow.Day,
                2,
                30,
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
                _logger.LogError(ex, "Unexpected error during SSI symbol import cycle. Trigger={Trigger}", trigger);
            }
        }

        private async Task RunImportCycleAsync(string trigger, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            _logger.LogInformation("Starting SSI symbol import cycle. Trigger={Trigger}", trigger);

            var result = await mediator.Send(new ImportSymbolsFromSsiCommandV2(), cancellationToken);

            _logger.LogInformation(
                "SSI symbol import cycle completed. Trigger={Trigger}, Imported={ImportedCount}, Updated={UpdatedCount}, Message={Message}",
                trigger,
                result.ImportedCount,
                result.UpdatedCount,
                result.Message);
        }
    }
}
