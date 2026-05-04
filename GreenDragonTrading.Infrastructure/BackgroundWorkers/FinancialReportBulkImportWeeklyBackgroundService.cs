using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportFromDnse;
using GreenDragonTrading.Domain.Constants.DNSE;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    /// <summary>
    /// Imports financial reports weekly on Sunday at 04:00 (GMT+7).
    /// </summary>
    public class FinancialReportBulkImportWeeklyBackgroundService : BackgroundService
    {
        private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

        private readonly ILogger<FinancialReportBulkImportWeeklyBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public FinancialReportBulkImportWeeklyBackgroundService(
            ILogger<FinancialReportBulkImportWeeklyBackgroundService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Weekly DNSE financial report import background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var delay = GetDelayUntilNextRun(DateTimeOffset.UtcNow);
                    _logger.LogInformation("Next DNSE financial report import scheduled after {Delay}.", delay);

                    await Task.Delay(delay, stoppingToken);
                    await RunImportCycleSafeAsync("weekly-sun-04:00", stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in DNSE financial report import schedule loop.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Weekly DNSE financial report import background service stopped.");
        }

        private static TimeSpan GetDelayUntilNextRun(DateTimeOffset utcNow)
        {
            var vnNow = utcNow.ToOffset(VnOffset);
            var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)vnNow.DayOfWeek + 7) % 7;

            var nextRun = new DateTimeOffset(
                vnNow.Year,
                vnNow.Month,
                vnNow.Day,
                4,
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
                _logger.LogError(ex, "Unexpected error during DNSE financial report import cycle. Trigger={Trigger}", trigger);
            }
        }

        private async Task RunImportCycleAsync(string trigger, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            _logger.LogInformation("Starting DNSE financial report import cycle. Trigger={Trigger}", trigger);

            var request = new ImportBulkFromDnseCommandV2(
                DnseConstants.CycleType.QUARTERLY,
                10);

            var result = await mediator.Send(request, cancellationToken);

            _logger.LogInformation(
                "DNSE financial report import cycle completed. Trigger={Trigger}, Success={Success}, Failed={Failed}, Message={Message}",
                trigger,
                result.Data?.SuccessCount ?? 0,
                result.Data?.FailedCount ?? 0,
                result.Message);
        }
    }
}
