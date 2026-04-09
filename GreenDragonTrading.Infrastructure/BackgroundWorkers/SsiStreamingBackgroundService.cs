using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Constants.SSI;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
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

            // Clear all intraday index lists and their DATE sentinel keys on every startup.
            // This guarantees the chart never shows stale data from a previous session,
            // regardless of whether the date-based reset in HandleIndexData already fired.
            using (var cleanupScope = _serviceScopeFactory.CreateScope())
            {
                var redis = cleanupScope.ServiceProvider.GetRequiredService<IRedisService>();
                var deletedCount = await redis.DeleteByPatternAsync(RedisConstants.IndexIntradayPattern());
                _logger.LogInformation(
                    "[Startup] Cleared {Count} stale INDEX:INTRADAY:* keys from Redis.",
                    deletedCount);
            }

            IEnumerable<string> tickers = [];
            IEnumerable<string> indexCodes = [];

            // Retrieve all tickers and index codes from the database
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                tickers = await _uow.Symbols.GetAllTickersAsync(stoppingToken);
                
                // Initialize symbol metadata cache
                await InitializeSymbolCacheAsync(_uow, stoppingToken);

                // Initialize index name cache
                await InitializeIndexCacheAsync(_uow, stoppingToken);

                // Load active market index codes from the database
                var loadedIndexCodes = _uow.MarketIndices
                    .GetQueryable()
                    .Where(i => i.Status == CommonStatus.Active)
                    .Select(i => i.Code.ToUpper())
                    .ToList();

                var duplicateCodes = loadedIndexCodes
                    .GroupBy(code => code, StringComparer.Ordinal)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .OrderBy(code => code)
                    .ToList();

                if (duplicateCodes.Count > 0)
                {
                    _logger.LogWarning(
                        "Detected duplicate active market index codes in DB: {Codes}. Duplicates were removed before MI subscription.",
                        string.Join(",", duplicateCodes));
                }

                indexCodes = loadedIndexCodes.Distinct(StringComparer.Ordinal).ToList();

                _logger.LogInformation(
                    "Loaded {UniqueCount} unique active market index codes from database (raw count: {RawCount})",
                    indexCodes.Count(),
                    loadedIndexCodes.Count);
            }

            string tickersString = string.Join("-", tickers);

            var filterList = new List<string>
            {
                $"{SsiConstantsV2.SSI_STREAMING_CHANNEL_X_TRADE}:{tickersString}",
                $"{SsiConstantsV2.SSI_STREAMING_CHANNEL_FOREIGN}:{tickersString}",
                $"{SsiConstantsV2.SSI_STREAMING_CHANNEL_X}:{tickersString}",
                $"{SsiConstantsV2.SSI_STREAMING_CHANNEL_B}:{tickersString}"
            };

            _logger.LogInformation("Prepared OHLCV/X/X-TRADE/FOREIGN filters for {Count} symbols", tickers.Count());

            // Subscribe to Channel MI: Realtime market index data
            if (indexCodes.Any())
            {
                string indexCodesString = string.Join("-", indexCodes);
                string indexFilter = $"{SsiConstantsV2.SSI_STREAMING_CHANNEL_MI}:{indexCodesString}";
                filterList.Add(indexFilter);
                _logger.LogInformation("Prepared Market Index (Channel MI) filter for {Count} indices: {Codes}",
                    indexCodes.Count(), indexCodesString);
            }
            else
            {
                _logger.LogWarning("No market index codes configured – skipping MI channel subscription");
            }

            // Switch to all channels simultaneously to prevent overwriting previous subscriptions
            string combinedFilter = string.Join(",", filterList);
            await _streamingService.SwitchChannelsAsync(combinedFilter);

            // Keep processing loops attached to hosted-service lifecycle.
            var redisWriteLoop = ProcessRedisWriteBatchAsync(stoppingToken);
            var broadcastLoop = ProcessBroadcastBatchAsync(stoppingToken);
            var candleSaveLoop = ProcessCandleSaveQueueAsync(stoppingToken);

            await Task.WhenAll(redisWriteLoop, broadcastLoop, candleSaveLoop);
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
