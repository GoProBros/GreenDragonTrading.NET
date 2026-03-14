using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants.SSI;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    /// <summary>
    /// Background service that handles SSI streaming events.
    /// Subscribes to streaming events once at startup and logs all received data.
    /// Broadcasts data to connected frontend clients via SignalR.
    /// </summary>
    public partial class SsiStreamingBackgroundService : BackgroundService
    {
        private readonly ISsiStreamingService _streamingService;
        private readonly ILogger<SsiStreamingBackgroundService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMarketDataBroadcaster _broadcaster;
        private readonly Func<string, Task> _broadcastHandler;
        private readonly Action<string> _errorHandler;
        private readonly Action<string, string> _stateChangedHandler;
        private bool _disposed;

        // Redis update locks to prevent race conditions
        // Key: "OHLCV:{ticker}:{timeframe}", Value: SemaphoreSlim for locking
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _redisLocks = new();

        // Bounded channel used to batch-save completed candles.
        // Producers (event handlers) enqueue candles; a single consumer loop bulk-upserts them.
        private readonly Channel<CurrentCandleDto> _saveCandleChannel =
            Channel.CreateBounded<CurrentCandleDto>(new BoundedChannelOptions(2000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            });

        /// <summary>
        /// Simple struct for passing OHLCV update data
        /// </summary>
        private record struct OhlcvUpdateData(
            decimal Open,
            decimal High,
            decimal Low,
            decimal Close,
            long Volume,
            decimal TotalValue);

        /// <summary>
        /// Constructor for SsiStreamingBackgroundService.
        /// </summary>
        /// <param name="streamingService">Define streaming service</param>
        /// <param name="logger">Logger</param>
        /// <param name="serviceScopeFactory">Service scope factory</param>
        /// <param name="broadcaster">Market data broadcaster</param>
        public SsiStreamingBackgroundService(
            ISsiStreamingService streamingService,
            ILogger<SsiStreamingBackgroundService> logger,
            IServiceScopeFactory serviceScopeFactory,
            IMarketDataBroadcaster broadcaster)
        {
            _streamingService = streamingService;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _broadcaster = broadcaster;

            _broadcastHandler = async (data) => await HandleBroadcast(data);
            _errorHandler = (error) => _ = HandleError(error);
            _stateChangedHandler = async (oldState, newState) => await HandleStateChanged(oldState, newState);

            _streamingService.OnBroadcastReceived += _broadcastHandler;
            _streamingService.OnErrorReceived += _errorHandler;
            _streamingService.OnStateChanged += _stateChangedHandler;
        }
        
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SSI Streaming Background Service started.");
            
            await _streamingService.StartAsync(stoppingToken);

            IEnumerable<string> tickers = [];

            // Retrieve all tickers from the database
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                tickers = await _uow.Symbols.GetAllTickersAsync(stoppingToken);
                
                // Initialize symbol metadata cache
                await InitializeSymbolCacheAsync(_uow, stoppingToken);
            }
            string tickersString = string.Join("-", tickers);

            // Subscribe to X-TRADE channel for all tickers
            string xTradeFilter = $"{SsiConstantsV2.SSI_STREAMING_CHANNEL_X_TRADE}:{tickersString}";
            await _streamingService.SwitchChannelsAsync(xTradeFilter);

            // Subscribe to Foreign Room channel for all tickers
            string foreignFilter = $"{SsiConstantsV2.SSI_STREAMING_CHANNEL_FOREIGN}:{tickersString}";
            await _streamingService.SwitchChannelsAsync(foreignFilter);

            string snapshotFilter = $"{SsiConstantsV2.SSI_STREAMING_CHANNEL_X}:{tickersString}";
            await _streamingService.SwitchChannelsAsync(snapshotFilter);

            // Subscribe to Channel B: Realtime OHLCV data (replaces tick aggregation)
            string ohlcvFilter = $"{SsiConstantsV2.SSI_STREAMING_CHANNEL_B}:{tickersString}";
            _logger.LogInformation("Subscribing to OHLCV (Channel B) for {Count} symbols", tickers.Count());
            await _streamingService.SwitchChannelsAsync(ohlcvFilter);

            // Start batched Redis write loop (100ms)
            _ = Task.Run(() => ProcessRedisWriteBatchAsync(stoppingToken), stoppingToken);

            // Start batched broadcast loop (100ms)
            _ = Task.Run(() => ProcessBroadcastBatchAsync(stoppingToken), stoppingToken);

            // Start the background candle-batch-save loop
            _ = Task.Run(() => ProcessCandleSaveQueueAsync(stoppingToken), stoppingToken);
        }

        /// <summary>
        /// Drains the candle save channel in batches and bulk-upserts them with a single DB scope.
        /// Keeps the connection count low regardless of how many candles close simultaneously.
        /// </summary>
        private async Task ProcessCandleSaveQueueAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Candle batch-save loop started.");
            var reader = _saveCandleChannel.Reader;
            var batch = new List<Ohlcv>(200);
            var now = DateTime.UtcNow;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Wait for at least one item
                    await reader.WaitToReadAsync(stoppingToken);

                    // Drain everything currently available (up to 200 items per save cycle)
                    batch.Clear();
                    now = DateTime.UtcNow;
                    while (batch.Count < 200 && reader.TryRead(out var candle))
                    {
                        batch.Add(new Ohlcv
                        {
                            Time        = candle.StartTime,
                            Ticker      = candle.Ticker,
                            Timeframe   = candle.Timeframe,
                            Open        = candle.Open,
                            High        = candle.High,
                            Low         = candle.Low,
                            Close       = candle.Close,
                            Volume      = candle.Volume,
                            Value       = candle.TotalValue,
                            CreatedAt   = now,
                            Source      = "SSI_STREAMING",
                            IsPreliminary = false
                        });
                    }

                    if (batch.Count == 0) continue;

                    using var scope = _serviceScopeFactory.CreateScope();
                    var ohlcvUow = scope.ServiceProvider.GetRequiredService<IOhlcvUnitOfWork>();
                    await ohlcvUow.Ohlcv.BulkUpsertAsync(batch, stoppingToken);

                    _logger.LogDebug("Batch-saved {Count} candles to DB.", batch.Count);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in candle batch-save loop.");
                    await Task.Delay(1000, stoppingToken); // brief back-off on error
                }
            }

            _logger.LogInformation("Candle batch-save loop stopped.");
        }

        /// <summary>
        /// Handle streaming errors.
        /// </summary>
        /// <param name="error">Error message</param>
        private Task HandleError(string error)
        {
            _logger.LogError("Streaming error: {Error}", error);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle streaming state changes.
        /// </summary>
        /// <param name="oldState">Previous connection state.</param>
        /// <param name="newState">New connection state.</param>
        private Task HandleStateChanged(string oldState, string newState)
        {
            _logger.LogInformation("Streaming connection state changed: {OldState} -> {NewState}", oldState, newState);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _streamingService.OnBroadcastReceived -= _broadcastHandler;
            _streamingService.OnErrorReceived -= _errorHandler;
            _streamingService.OnStateChanged -= _stateChangedHandler;

            foreach (var redisLock in _redisLocks.Values)
            {
                redisLock.Dispose();
            }
            _redisLocks.Clear();

            base.Dispose();
        }
    }
}
