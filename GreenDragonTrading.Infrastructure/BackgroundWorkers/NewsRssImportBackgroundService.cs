using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportNewsFromRss;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    /// <summary>
    /// Periodically imports stock news from RSS and persists new articles.
    /// </summary>
    public class NewsRssImportBackgroundService : BackgroundService
    {
        private static readonly TimeSpan ImportInterval = TimeSpan.FromHours(4);

        private readonly ILogger<NewsRssImportBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public NewsRssImportBackgroundService(
            ILogger<NewsRssImportBackgroundService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("News RSS import background service started.");

            await RunImportCycleSafeAsync("startup", stoppingToken);

            using var timer = new PeriodicTimer(ImportInterval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunImportCycleSafeAsync("scheduled-4h", stoppingToken);
            }

            _logger.LogInformation("News RSS import background service stopped.");
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
                _logger.LogError(ex, "Unexpected error during RSS news import cycle. Trigger={Trigger}", trigger);
            }
        }

        private async Task RunImportCycleAsync(string trigger, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            _logger.LogInformation("Starting RSS news import cycle. Trigger={Trigger}", trigger);

            var result = await mediator.Send(new ImportNewsFromRssCommand(), cancellationToken);

            _logger.LogInformation(
                "RSS news import cycle completed. Trigger={Trigger}, Fetched={FetchedCount}, Inserted={InsertedCount}, Duplicated={DuplicatedCount}, Message={Message}",
                trigger,
                result.FetchedCount,
                result.InsertedCount,
                result.DuplicatedCount,
                result.Message);
        }
    }
}
