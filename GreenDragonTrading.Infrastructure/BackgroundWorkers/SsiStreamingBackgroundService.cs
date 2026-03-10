using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Constants.SSI;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    /// <summary>
    /// Background service that handles SSI streaming events.
    /// Subscribes to streaming events once at startup and logs all received data.
    /// Broadcasts data to connected frontend clients via SignalR.
    /// </summary>
    public class SsiStreamingBackgroundService : BackgroundService
    {
        private readonly ISsiStreamingService _streamingService;
        private readonly ILogger<SsiStreamingBackgroundService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMarketDataBroadcaster _broadcaster;
        private readonly Func<string, Task> _broadcastHandler;
        private readonly Action<string> _errorHandler;
        private readonly Action<string, string> _stateChangedHandler;

        // Symbol metadata cache to avoid DB queries on every tick
        // Loaded once at startup and reused for heatmap broadcasting
        private readonly ConcurrentDictionary<string, SymbolMetadata> _symbolMetadataCache = new();

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

        // No more state dictionaries! Redis is the single source of truth
        // All candle states stored in Redis with key pattern: OHLCV:{ticker}:{timeframe}

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
            _errorHandler = async (error) => await HandleError(error);
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

            // Subscribe to X-QUOTE channel for all tickers
            string xQuoteFilter = $"{SsiConstantsV2.SSI_STREAMING_CHANNEL_X_QUOTE}:{tickersString}";
            await _streamingService.SwitchChannelsAsync(xQuoteFilter);

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
        /// Initialize symbol metadata cache from database
        /// Eliminates need for DB queries during tick processing
        /// </summary>
        private async Task InitializeSymbolCacheAsync(IUnitOfWork uow, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Initializing symbol metadata cache...");

                // Get all active symbols for heatmap
                var symbols = await uow.Symbols.GetActiveSymbolsForHeatmapAsync(
                    exchange: null,
                    sectorIds: null,
                    cancellationToken);

                foreach (var symbol in symbols)
                {
                    var metadata = new SymbolMetadata
                    {
                        Ticker = symbol.Ticker,
                        CompanyName = symbol.ViCompanyName ?? symbol.EnCompanyName ?? string.Empty,
                        Exchange = symbol.ExchangeCode,
                        SectorId = symbol.SectorId,
                        SectorName = symbol.Sector?.ViName ?? symbol.Sector?.EnName
                    };
                    _symbolMetadataCache.TryAdd(symbol.Ticker.ToUpper(), metadata);
                }

                _logger.LogInformation(
                    "Symbol metadata cache initialized with {Count} symbols",
                    _symbolMetadataCache.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing symbol metadata cache");
            }
        }

        /// <summary>
        /// Handle incoming broadcast data.
        /// </summary>
        /// <param name="data">Data received from the streaming service</param>
        private async Task HandleBroadcast(string data)
        {
            try
            {
                StreamingWrapperResponse wrapperResponse = JsonSerializer.Deserialize<StreamingWrapperResponse>(data)!;

                using var scope = _serviceScopeFactory.CreateScope();
                var _redis = scope.ServiceProvider.GetRequiredService<IRedisService>();

                if (string.Equals(wrapperResponse.DataType, SsiConstantsV2.SSI_STREAMING_DATA_TYPE_X_QUOTE))
                {
                    var response = JsonSerializer.Deserialize<XQuoteResponse>(wrapperResponse.Content!);
                    //await HandleXQuote(_redis, response);
                }
                else if (string.Equals(wrapperResponse.DataType, SsiConstantsV2.SSI_STREAMING_DATA_TYPE_X_TRADE))
                {
                    var response = JsonSerializer.Deserialize<XTradeResponse>(wrapperResponse.Content!);
                    //_logger.LogInformation(wrapperResponse.Content);
                    await HandleXTrade(_redis, response);
                }
                else if (string.Equals(wrapperResponse.DataType, SsiConstantsV2.SSI_STREAMING_DATA_TYPE_FOREIGN))
                {
                    var response = JsonSerializer.Deserialize<ForeignRoomResponse>(wrapperResponse.Content!);
                    await HandleForeignRoom(_redis, response);
                }
                else if (string.Equals(wrapperResponse.DataType, SsiConstantsV2.SSI_STREAMING_DATA_TYPE_X))
                {
                    var response = JsonSerializer.Deserialize<SecuritiesSnapshot>(wrapperResponse.Content!);
                    await HandleSnapshot(_redis, response);
                }
                else if (string.Equals(wrapperResponse.DataType, SsiConstantsV2.SSI_STREAMING_DATA_TYPE_B))
                {
                    var response = JsonSerializer.Deserialize<OhlcvDataResponse>(wrapperResponse.Content!);
                    await HandleOhlcvData(_redis, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing streaming data: {Data}", data);
            }
        }

        /// <summary>
        /// Handle streaming errors.
        /// </summary>
        /// <param name="error">Error message</param>
        private Task HandleError(string error)
        {
            _logger.LogError("SSI Streaming error: {Error}", error);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle state changes in the streaming connection.
        /// </summary>
        /// <param name="oldState">Old streaming state</param>
        /// <param name="newState">New streaming state</param>
        /// <returns></returns>
        private Task HandleStateChanged(string oldState, string newState)
        {
            _logger.LogInformation("SSI connection state changed: {OldState} -> {NewState}", oldState, newState);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Dispose the background service and unsubscribe from events.
        /// </summary>
        public override void Dispose()
        {
            _streamingService.OnBroadcastReceived -= _broadcastHandler;
            _streamingService.OnErrorReceived -= _errorHandler;
            _streamingService.OnStateChanged -= _stateChangedHandler;

            base.Dispose();
        }

        #region Handle X-QUOTE

        /// <summary>
        /// Handle X-QUOTE data from SSI streaming service.
        /// </summary>
        /// <param name="redis">Redis service</param>
        /// <param name="response">X-QUOTE data from SSI</param>
        private async Task HandleXQuote(IRedisService redis, XQuoteResponse? response)
        {
            try
            {
                string redisKey = $"{RedisConstants.REDIS_KEY_PREFIX_MARKET_DATA}:{response!.Symbol}";

                if (await redis.ExistsAsync(redisKey))
                {
                    await UpdateExistingQuoteData(redis, redisKey, response);
                }
                else
                {
                    await CreateNewQuoteData(redis, redisKey, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling X-QUOTE data for symbol: {Symbol}", response?.Symbol);
            }
        }

        /// <summary>
        /// Update existing quote data of symbol in Redis.
        /// </summary>
        /// <param name="redis">Redis service</param>
        /// <param name="redisKey">Redis key to retrieve existing symbol data</param>
        /// <param name="response">X-QUOTE data from SSI</param>
        private async Task UpdateExistingQuoteData(IRedisService redis, string redisKey, XQuoteResponse response)
        {
            var existingData = (await redis.GetHashAsync<MarketSymbolDto>(redisKey))!;
            var updates = new Dictionary<string, object>();

            AddIfChanged(updates, nameof(MarketSymbolDto.BidPrice1), response.BidPrice1, existingData.BidPrice1);
            AddIfChanged(updates, nameof(MarketSymbolDto.BidVol1), response.BidVol1, existingData.BidVol1);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskPrice1), response.AskPrice1, existingData.AskPrice1);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskVol1), response.AskVol1, existingData.AskVol1);
            AddIfChanged(updates, nameof(MarketSymbolDto.BidPrice2), response.BidPrice2, existingData.BidPrice2);
            AddIfChanged(updates, nameof(MarketSymbolDto.BidVol2), response.BidVol2, existingData.BidVol2);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskPrice2), response.AskPrice2, existingData.AskPrice2);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskVol2), response.AskVol2, existingData.AskVol2);
            AddIfChanged(updates, nameof(MarketSymbolDto.BidPrice3), response.BidPrice3, existingData.BidPrice3);
            AddIfChanged(updates, nameof(MarketSymbolDto.BidVol3), response.BidVol3, existingData.BidVol3);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskPrice3), response.AskPrice3, existingData.AskPrice3);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskVol3), response.AskVol3, existingData.AskVol3);

            if (updates.Count > 0)
            {
                await redis.SetHashFieldsAsync(redisKey, updates);
                updates["Ticker"] = response.Symbol!;

                // Broadcast market data to ReceiveMarketData listeners
                await SafeBroadcastAsync(
                    () => _broadcaster.BroadcastMarketDataAsync(response.Symbol!, updates),
                    $"market data for {response.Symbol}");

                // Re-read full snapshot to build PriceDepthDto with all fields (ref, ceiling, etc.)
                var fullData = await redis.GetHashAsync<MarketSymbolDto>(redisKey);
                if (fullData != null)
                {
                    var depth = BuildPriceDepthDto(response.Symbol!, fullData);
                    await SafeBroadcastAsync(
                        () => _broadcaster.BroadcastPriceDepthAsync(depth),
                        $"price depth for {response.Symbol}");
                }
            }
        }

        /// <summary>
        /// When no existing quote data, create new entry in Redis.
        /// </summary>
        /// <param name="redis">Redis service</param>
        /// <param name="redisKey">Redis key to store new symbol data</param>
        /// <param name="response">X-QUOTE data from SSI</param>
        private async Task CreateNewQuoteData(IRedisService redis, string redisKey, XQuoteResponse response)
        {
            var newData = new MarketSymbolDto
            {
                Ticker = response.Symbol!,
                BidPrice1 = response.BidPrice1 ?? default,
                BidVol1 = response.BidVol1 ?? default,
                AskPrice1 = response.AskPrice1 ?? default,
                AskVol1 = response.AskVol1 ?? default,
                BidPrice2 = response.BidPrice2 ?? default,
                BidVol2 = response.BidVol2 ?? default,
                AskPrice2 = response.AskPrice2 ?? default,
                AskVol2 = response.AskVol2 ?? default,
                BidPrice3 = response.BidPrice3 ?? default,
                BidVol3 = response.BidVol3 ?? default,
                AskPrice3 = response.AskPrice3 ?? default,
                AskVol3 = response.AskVol3 ?? default,
            };
            await redis.SetHashAsync(redisKey, newData);

            // Broadcast market data
            await SafeBroadcastAsync(
                () => _broadcaster.BroadcastMarketDataAsync(response.Symbol!, newData),
                $"new quote data for {response.Symbol}");

            // Broadcast price depth (limited data on new creation, no ref/ceiling yet)
            var depth = BuildPriceDepthDto(response.Symbol!, newData);
            await SafeBroadcastAsync(
                () => _broadcaster.BroadcastPriceDepthAsync(depth),
                $"price depth for {response.Symbol}");
        }

        #endregion Handle X-QUOTE

        #region Handle X-TRADE

        /// <summary>
        /// Handle X-TRADE data from SSI streaming service.
        /// </summary>
        /// <param name="redis">Redis service</param>
        /// <param name="response">X-TRADE data from SSI</param>
        private async Task HandleXTrade(IRedisService redis, XTradeResponse? response)
        {
            try
            {
                string redisKey = $"{RedisConstants.REDIS_KEY_PREFIX_MARKET_DATA}:{response!.Symbol}";

                if (await redis.ExistsAsync(redisKey))
                {
                    await UpdateExistingTradeData(redis, redisKey, response);
                }
                else
                {
                    await CreateNewTradeData(redis, redisKey, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling X-TRADE data for symbol: {Symbol}", response?.Symbol);
            }
        }

        /// <summary>
        /// Update existing trade data of symbol in Redis.
        /// </summary>
        /// <param name="redis">Redis service</param>
        /// <param name="redisKey">Redis key to retrieve symbol data</param>
        /// <param name="response">X-TRADE data from SSI</param>
        private async Task UpdateExistingTradeData(IRedisService redis, string redisKey, XTradeResponse response)
        {
            var existingData = (await redis.GetHashAsync<MarketSymbolDto>(redisKey))!;
            var updates = new Dictionary<string, object>();

            AddIfChanged(updates, nameof(MarketSymbolDto.CeilingPrice), response.Ceiling, existingData.CeilingPrice);
            AddIfChanged(updates, nameof(MarketSymbolDto.FloorPrice), response.Floor, existingData.FloorPrice);
            AddIfChanged(updates, nameof(MarketSymbolDto.ReferencePrice), response.RefPrice, existingData.ReferencePrice);
            AddIfChanged(updates, nameof(MarketSymbolDto.LastPrice), response.LastPrice, existingData.LastPrice);
            AddIfChanged(updates, nameof(MarketSymbolDto.LastVol), response.LastVol, existingData.LastVol);
            AddIfChanged(updates, nameof(MarketSymbolDto.TotalVal), response.TotalVal, existingData.TotalVal);
            AddIfChanged(updates, nameof(MarketSymbolDto.TotalVol), response.TotalVol, existingData.TotalVol);
            AddIfChanged(updates, nameof(MarketSymbolDto.Change), response.Change, existingData.Change);
            AddIfChanged(updates, nameof(MarketSymbolDto.RatioChange), response.RatioChange, existingData.RatioChange);
            AddIfChanged(updates, nameof(MarketSymbolDto.Highest), response.Highest, existingData.Highest);
            AddIfChanged(updates, nameof(MarketSymbolDto.Lowest), response.Lowest, existingData.Lowest);
            AddIfChanged(updates, nameof(MarketSymbolDto.Side), response.Side, existingData.Side);
            AddIfChanged(updates, nameof(MarketSymbolDto.AvgPrice), response.AvgPrice, existingData.AvgPrice);
            AddIfChanged(updates, nameof(MarketSymbolDto.PriorVal), response.PriorVal, existingData.PriorVal);

            // Calculate volume delta to detect new matched orders.
            // Use TotalVol diff instead of LastVol because SSI can batch multiple orders
            // in one broadcast: TotalVol increases by 50,000 but LastVol only shows the
            // final order = 10,000 → using LastVol would miss 40,000 volume.
            var newTotalVol = (long)(response.TotalVol ?? 0);

            // REMOVED: isSessionReset check was causing false positives.
            // Original intent: Detect when SSI resets TotalVol to 0 at start of new trading day.
            // Problem: Condition `newTotalVol < existingData.TotalVol` is too broad and triggers
            // when SSI broadcasts arrive out-of-order (race between Snapshot vs X-Trade channels),
            // causing volDelta=0 incorrectly → blocking real trades from being recorded.
            // Solution: Math.Max(0, ...) already handles all cases safely:
            //   - New trade: TotalVol increases → volDelta > 0 → record trade ✅
            //   - SSI replay on reconnect: TotalVol unchanged → volDelta = 0 → skip ✅  
            //   - New session (TotalVol reset to 0): Math.Max(0, negative) = 0 → skip ✅
            // bool isSessionReset = newTotalVol < existingData.TotalVol && existingData.TotalVol > 1000;

            long volDelta;
            long newBuyVol;
            long newSellVol;

            // NOTE: isSessionReset logic removed (see comment block above).
            // Math.Max(0, ...) handles all edge cases including session resets.
            volDelta   = Math.Max(0L, newTotalVol - (long)existingData.TotalVol);
            newBuyVol  = (long)existingData.TotalBuyVol;
            newSellVol = (long)existingData.TotalSellVol;

            var sideUpper = (response.Side ?? string.Empty).ToUpperInvariant();
            if (volDelta > 0)
            {
                if (sideUpper.StartsWith("B"))
                    newBuyVol += volDelta;
                else if (sideUpper.StartsWith("S") || sideUpper.StartsWith("M"))
                    newSellVol += volDelta;
                else
                {
                    // Diagnostic: log unrecognized Side to identify what SSI actually sends
                    _logger.LogWarning(
                        "Unrecognized Side='{Side}' for {Ticker} volDelta={Vol} — not counted in Buy/Sell",
                        response.Side, response.Symbol, volDelta);
                }
            }
            // Always write both fields so old hashes that pre-date these fields get them initialised
            updates[nameof(MarketSymbolDto.TotalBuyVol)]  = newBuyVol;
            updates[nameof(MarketSymbolDto.TotalSellVol)] = newSellVol;

            if (updates.Count > 0)
            {
                await redis.SetHashFieldsAsync(redisKey, updates);
                updates["Ticker"] = response.Symbol!;

                // Broadcast updated fields (LastPrice, TotalVol, TotalBuyVol, TotalSellVol …)
                // so ReceiveMarketData listeners on the frontend stay in sync.
                await SafeBroadcastAsync(
                    () => _broadcaster.BroadcastMarketDataAsync(response.Symbol!, updates),
                    $"market data (trade) for {response.Symbol}");

                // Re-read the full snapshot and broadcast PriceDepth so the
                // "3 Bước Giá" module receives updated Tổng mua / Tổng bán values.
                var fullData = await redis.GetHashAsync<MarketSymbolDto>(redisKey);
                if (fullData != null)
                {
                    var depthDto = BuildPriceDepthDto(response.Symbol!, fullData);
                    await SafeBroadcastAsync(
                        () => _broadcaster.BroadcastPriceDepthAsync(depthDto),
                        $"price depth (trade) for {response.Symbol}");
                }
            }

            // Only record a trade entry when TotalVol actually increased (volDelta > 0).
            // SSI replays the last trade on every reconnect — volDelta will be 0 in that case.
            // Math.Max(0, ...) above ensures volDelta=0 for all non-trade scenarios:
            //   - Replay: same TotalVol → diff=0 → volDelta=0 ✅
            //   - Session reset: negative diff → Math.Max(0, neg)=0 ✅
            //   - Out-of-order broadcast: negative diff → Math.Max(0, neg)=0 ✅
            var isNewTrade  = response.LastPrice.HasValue
                              && response.LastVol.HasValue
                              && response.LastPrice > 0
                              && volDelta > 0;

            if (isNewTrade)
            {
                // Parse SSI time field "HHmmss" (UTC+7) → "HH:mm:ss".
                // Fall back to current UTC+7 if the field is missing or malformed.
                var rawTime = response.Time;
                string formattedTime;
                if (!string.IsNullOrWhiteSpace(rawTime) && rawTime.Length >= 6 && rawTime.All(char.IsDigit))
                    formattedTime = $"{rawTime[0..2]}:{rawTime[2..4]}:{rawTime[4..6]}";
                else
                    formattedTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("HH:mm:ss");

                var trade = new RecentTradeDto
                {
                    Ticker = response.Symbol!,
                    Price = response.LastPrice!.Value,
                    Volume = response.LastVol!.Value,
                    Side = response.Side ?? string.Empty,
                    Time = formattedTime
                };
                await redis.ListPushTrimAsync($"TRADES:{response.Symbol!.ToUpper()}", trade, 200);
                await SafeBroadcastAsync(
                    () => _broadcaster.BroadcastTradeAsync(trade),
                    $"trade for {response.Symbol}");
            }
        }

        /// <summary>
        /// If no existing trade data, create new entry in Redis.
        /// </summary>
        /// <param name="redis">Redis service</param>
        /// <param name="redisKey">Redis key to retrieve symbol data</param>
        /// <param name="response">X-TRADE data from SSI</param>
        private async Task CreateNewTradeData(IRedisService redis, string redisKey, XTradeResponse response)
        {
            var newData = new MarketSymbolDto
            {
                Ticker = response.Symbol!,
                CeilingPrice = response.Ceiling ?? default,
                FloorPrice = response.Floor ?? default,
                ReferencePrice = response.RefPrice ?? default,
                LastPrice = response.LastPrice ?? default,
                LastVol = response.LastVol ?? default,
                TotalVal = response.TotalVal ?? default,
                TotalVol = response.TotalVol ?? default,
                Change = response.Change ?? default,
                RatioChange = response.RatioChange ?? default,
                Highest = response.Highest ?? default,
                Lowest = response.Lowest ?? default,
                Side = response.Side ?? string.Empty,
                AvgPrice = response.AvgPrice ?? default,
                PriorVal = response.PriorVal ?? default,
                // SSI gửi Side dạng multi-char: "BU"/"BD" = Buy, "SD"/"SU" = Sell, "M" = cũ.
                TotalBuyVol  = (response.Side ?? string.Empty).ToUpperInvariant().StartsWith("B") ? (response.LastVol ?? 0) : 0,
                TotalSellVol = ((response.Side ?? string.Empty).ToUpperInvariant().StartsWith("S") ||
                                (response.Side ?? string.Empty).ToUpperInvariant().StartsWith("M")) ? (response.LastVol ?? 0) : 0,
            };
            await redis.SetHashAsync(redisKey, newData);
            // NOTE: Do NOT push to TRADES list here.
            // CreateNewTradeData only initializes the Redis hash with the first X-Trade payload
            // (SSI replays last known data on reconnect). Writing to TRADES here would create
            // a spurious record every time the server restarts or the key is missing.
            // Real trade records are only written in UpdateExistingTradeData.
        }

        #endregion Handle X-TRADE

        #region Handle Foreign Room

        /// <summary>
        /// Handle Foreign Room data from SSI streaming service.
        /// </summary>
        /// <param name="redis">Redis service</param>
        /// <param name="response">Foreign Room response data</param>
        private async Task HandleForeignRoom(IRedisService redis, ForeignRoomResponse? response)
        {
            try
            {
                string redisKey = $"{RedisConstants.REDIS_KEY_PREFIX_MARKET_DATA}:{response!.Symbol}";

                if (await redis.ExistsAsync(redisKey))
                {
                    await UpdateExistingForeignData(redis, redisKey, response);
                }
                else
                {
                    await CreateNewForeignData(redis, redisKey, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling X-TRADE data for symbol: {Symbol}", response?.Symbol);
            }
        }

        /// <summary>
        /// Update existing foreign data of symbol in Redis.
        /// </summary>
        /// <param name="redis">Redis service</param>
        /// <param name="redisKey">Redis key to retrieve symbol data from redis</param>
        /// <param name="response">Foreign data from SSI</param>
        private async Task UpdateExistingForeignData(IRedisService redis, string redisKey, ForeignRoomResponse response)
        {
            var existingData = (await redis.GetHashAsync<MarketSymbolDto>(redisKey))!;
            var updates = new Dictionary<string, object>();

            AddIfChanged(updates, nameof(MarketSymbolDto.TotalRoom), response.TotalRoom, existingData.TotalRoom);
            AddIfChanged(updates, nameof(MarketSymbolDto.CurrentRoom), response.CurrentRoom, existingData.CurrentRoom);
            AddIfChanged(updates, nameof(MarketSymbolDto.FBuyVol), response.FBuyVol, existingData.FBuyVol);
            AddIfChanged(updates, nameof(MarketSymbolDto.FSellVol), response.FSellVol, existingData.FSellVol);
            AddIfChanged(updates, nameof(MarketSymbolDto.FSellVal), response.FSellVal, existingData.FSellVal);
            AddIfChanged(updates, nameof(MarketSymbolDto.FBuyVal), response.FBuyVal, existingData.FBuyVal);

            if (updates.Count > 0)
            {
                await redis.SetHashFieldsAsync(redisKey, updates);
            }

            updates["Ticker"] = response.Symbol!;
            
            // Broadcast in background - don't let failures block data updates
            await SafeBroadcastAsync(
                () => _broadcaster.BroadcastMarketDataAsync(response.Symbol!, updates),
                $"foreign room data for {response.Symbol}");
        }

        /// <summary>
        /// If no existing trade data, create new entry in Redis.
        /// </summary>
        /// <param name="redis">Redis service</param>
        /// <param name="redisKey">Redis key to retrieve symbol data from redis</param>
        /// <param name="response">Foreign data from SSI</param>
        private async Task CreateNewForeignData(IRedisService redis, string redisKey, ForeignRoomResponse response)
        {
            var newData = new MarketSymbolDto
            {
                Ticker = response.Symbol!,
                TotalRoom = response.TotalRoom ?? default,
                CurrentRoom = response.CurrentRoom ?? default,
                FBuyVol = response.FBuyVol ?? default,
                FSellVol = response.FSellVol ?? default,
                FBuyVal = response.FBuyVal ?? default,
                FSellVal = response.FSellVal ?? default,
            };
            await redis.SetHashAsync(redisKey, newData);
            
            // Broadcast in background - don't let failures block data updates
            await SafeBroadcastAsync(
                () => _broadcaster.BroadcastMarketDataAsync(response.Symbol!, newData),
                $"new foreign room data for {response.Symbol}");
        }

        #endregion Handle Foreign Room

        #region Handle Snapshot

        /// <summary>
        /// Handle Snapshot data from SSI streaming service.
        /// </summary>
        /// <param name="redis">Redis service</param>
        /// <param name="response">Snapshot response data</param>
        private async Task HandleSnapshot(IRedisService redis, SecuritiesSnapshot? response)
        {
            try
            {
                string redisKey = $"{RedisConstants.REDIS_KEY_PREFIX_MARKET_DATA}:{response!.Symbol}";

                if (await redis.ExistsAsync(redisKey))
                {
                    await UpdateExistingSnapshotData(redis, redisKey, response);
                }
                else
                {
                    await CreateNewSnapshotData(redis, redisKey, response);
                }
            }
            catch (TaskCanceledException)
            {
                // Task canceled during shutdown - this is expected, ignore silently
                return;
            }
            catch (OperationCanceledException)
            {
                // Operation canceled during shutdown - this is expected, ignore silently
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Snapshot data for symbol: {Symbol}", response?.Symbol);
            }
        }

        /// <summary>
        /// Update existing snapshot data of symbol in Redis.
        /// </summary>
        /// <param name="redis">Redis service</param>
        /// <param name="redisKey">Redis key to retrieve symbol data from redis</param>
        /// <param name="response">Snapshot data from SSI</param>
        private async Task UpdateExistingSnapshotData(IRedisService redis, string redisKey, SecuritiesSnapshot response)
        {
            var existingData = (await redis.GetHashAsync<MarketSymbolDto>(redisKey))!;
            var updates = new Dictionary<string, object>();

            // Price data
            AddIfChanged(updates, nameof(MarketSymbolDto.CeilingPrice), response.Ceiling, existingData.CeilingPrice);
            AddIfChanged(updates, nameof(MarketSymbolDto.FloorPrice), response.Floor, existingData.FloorPrice);
            AddIfChanged(updates, nameof(MarketSymbolDto.ReferencePrice), response.RefPrice, existingData.ReferencePrice);
            AddIfChanged(updates, nameof(MarketSymbolDto.LastPrice), response.LastVal, existingData.LastPrice);
            AddIfChanged(updates, nameof(MarketSymbolDto.LastVol), response.LastVol, existingData.LastVol);
            //AddIfChanged(updates, nameof(MarketSymbolDto.AvgPrice), response.Avg, existingData.AvgPrice);
            AddIfChanged(updates, nameof(MarketSymbolDto.PriorVal), response.PriorVal, existingData.PriorVal);
            AddIfChanged(updates, nameof(MarketSymbolDto.Highest), response.High, existingData.Highest);
            AddIfChanged(updates, nameof(MarketSymbolDto.Lowest), response.Low, existingData.Lowest);
            AddIfChanged(updates, nameof(MarketSymbolDto.Change), response.Change, existingData.Change);
            AddIfChanged(updates, nameof(MarketSymbolDto.RatioChange), response.RatioChange, existingData.RatioChange);

            // Volume data
            AddIfChanged(updates, nameof(MarketSymbolDto.TotalVal), response.TotalVal, existingData.TotalVal);
            AddIfChanged(updates, nameof(MarketSymbolDto.TotalVol), response.TotalVol, existingData.TotalVol);

            // Bid prices and volumes
            AddIfChanged(updates, nameof(MarketSymbolDto.BidPrice1), response.BidPrice1, existingData.BidPrice1);
            AddIfChanged(updates, nameof(MarketSymbolDto.BidVol1), response.BidVol1, existingData.BidVol1);
            AddIfChanged(updates, nameof(MarketSymbolDto.BidPrice2), response.BidPrice2, existingData.BidPrice2);
            AddIfChanged(updates, nameof(MarketSymbolDto.BidVol2), response.BidVol2, existingData.BidVol2);
            AddIfChanged(updates, nameof(MarketSymbolDto.BidPrice3), response.BidPrice3, existingData.BidPrice3);
            AddIfChanged(updates, nameof(MarketSymbolDto.BidVol3), response.BidVol3, existingData.BidVol3);

            // Ask prices and volumes
            AddIfChanged(updates, nameof(MarketSymbolDto.AskPrice1), response.AskPrice1, existingData.AskPrice1);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskVol1), response.AskVol1, existingData.AskVol1);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskPrice2), response.AskPrice2, existingData.AskPrice2);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskVol2), response.AskVol2, existingData.AskVol2);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskPrice3), response.AskPrice3, existingData.AskPrice3);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskVol3), response.AskVol3, existingData.AskVol3);

            // Trading session and status
            AddIfChanged(updates, nameof(MarketSymbolDto.TradingSession), response.TradingSession, existingData.TradingSession ?? string.Empty);
            AddIfChanged(updates, nameof(MarketSymbolDto.TradingStatus), response.TradingStatus, existingData.TradingStatus ?? string.Empty);
            // NOTE: Side is NOT updated from Snapshot — only X-Trade sets Side ("M"=Mua/Buy, "B"=Bán/Sell)

            // Always write TotalBuyVol/TotalSellVol to ensure these fields exist in Redis
            // (old hashes created before these fields were added will not have them)
            updates[nameof(MarketSymbolDto.TotalBuyVol)]  = existingData.TotalBuyVol;
            updates[nameof(MarketSymbolDto.TotalSellVol)] = existingData.TotalSellVol;

            if (updates.Count > 0)
            {
                await redis.SetHashFieldsAsync(redisKey, updates);
                
                // Update Heatmap Redis key (single source of truth)
                // Returns true if heatmap actually changed (price, volume, etc)
                var heatmapChanged = await UpdateHeatmapRedisAsync(redis, response.Symbol!, updates, existingData);
                
                // Only broadcast if heatmap-relevant fields changed (not just Bid/Ask)
                if (heatmapChanged)
                {
                    await BroadcastHeatmapItemFromRedisAsync(redis, response.Symbol!);
                }
            }

            updates["Ticker"] = response.Symbol!;
            
            // Broadcast market data to ReceiveMarketData listeners
            await SafeBroadcastAsync(
                () => _broadcaster.BroadcastMarketDataAsync(response.Symbol!, updates),
                $"snapshot data for {response.Symbol}");

            // Broadcast fresh price depth from full Redis entry
            var latestData = await redis.GetHashAsync<MarketSymbolDto>(redisKey);
            if (latestData != null)
            {
                var depth = BuildPriceDepthDto(response.Symbol!, latestData);
                await SafeBroadcastAsync(
                    () => _broadcaster.BroadcastPriceDepthAsync(depth),
                    $"price depth for {response.Symbol}");
            }
        }

        /// <summary>
        /// If no existing snapshot data, create new entry in Redis.
        /// </summary>
        /// <param name="redis">Redis service</param>
        /// <param name="redisKey">Redis key to store symbol data in redis</param>
        /// <param name="response">Snapshot data from SSI</param>
        private async Task CreateNewSnapshotData(IRedisService redis, string redisKey, SecuritiesSnapshot response)
        {
            var newData = new MarketSymbolDto
            {
                Ticker = response.Symbol!,
                CeilingPrice = response.Ceiling ?? default,
                FloorPrice = response.Floor ?? default,
                ReferencePrice = response.RefPrice ?? default,
                LastPrice = response.LastVal ?? default,
                LastVol = response.LastVol ?? default,
                //AvgPrice = response.Avg ?? default,
                PriorVal = response.PriorVal ?? default,
                Highest = response.High ?? default,
                Lowest = response.Low ?? default,
                Change = response.Change ?? default,
                RatioChange = response.RatioChange ?? default,
                TotalVal = response.TotalVal ?? default,
                TotalVol = response.TotalVol ?? default,
                BidPrice1 = response.BidPrice1 ?? default,
                BidVol1 = response.BidVol1 ?? default,
                BidPrice2 = response.BidPrice2 ?? default,
                BidVol2 = response.BidVol2 ?? default,
                BidPrice3 = response.BidPrice3 ?? default,
                BidVol3 = response.BidVol3 ?? default,
                AskPrice1 = response.AskPrice1 ?? default,
                AskVol1 = response.AskVol1 ?? default,
                AskPrice2 = response.AskPrice2 ?? default,
                AskVol2 = response.AskVol2 ?? default,
                AskPrice3 = response.AskPrice3 ?? default,
                AskVol3 = response.AskVol3 ?? default,
                TradingSession = response.TradingSession ?? string.Empty,
                TradingStatus = response.TradingStatus ?? string.Empty,
                Side = response.Side ?? string.Empty
            };
            await redis.SetHashAsync(redisKey, newData);
            
            // Create initial Heatmap Redis entry
            await CreateInitialHeatmapRedisAsync(redis, response.Symbol!, newData);
            
            // Broadcast heatmap item to SignalR from Redis
            await BroadcastHeatmapItemFromRedisAsync(redis, response.Symbol!);
            
            // Broadcast market data
            await SafeBroadcastAsync(
                () => _broadcaster.BroadcastMarketDataAsync(response.Symbol!, newData),
                $"new snapshot data for {response.Symbol}");

            // Broadcast initial price depth
            var depth = BuildPriceDepthDto(response.Symbol!, newData);
            await SafeBroadcastAsync(
                () => _broadcaster.BroadcastPriceDepthAsync(depth),
                $"price depth for {response.Symbol}");
        }

        #endregion Handle Snapshot

        #region Helper Methods

        /// <summary>
        /// Safely execute broadcast operations without letting failures crash data processing.
        /// Broadcasts are fire-and-forget with error logging only.
        /// </summary>
        /// <param name="broadcastAction">The broadcast action to execute</param>
        /// <param name="context">Context description for logging</param>
        private async Task SafeBroadcastAsync(Func<Task> broadcastAction, string context)
        {
            try
            {
                await broadcastAction();
            }
            catch (Exception ex)
            {
                // Log but don't throw - broadcast failures should not stop data processing
                _logger.LogWarning(ex, "Broadcast failed for {Context}. Data saved but clients not notified.", context);
            }
        }

        /// <summary>
        /// Build a PriceDepthDto from a MarketSymbolDto, computing per-level change vs reference.
        /// </summary>
        private static PriceDepthDto BuildPriceDepthDto(string ticker, MarketSymbolDto d)
        {
            var refPrice = d.ReferencePrice;

            double Chg(double price) => price > 0 && refPrice > 0 ? price - refPrice : 0;
            double ChgPct(double price) => price > 0 && refPrice > 0 ? ((price - refPrice) / refPrice) * 100 : 0;

            // Bull / Bear depth ratio
            var bidSum = d.BidVol1 + d.BidVol2 + d.BidVol3;
            var askSum = d.AskVol1 + d.AskVol2 + d.AskVol3;
            var depthTotal = bidSum + askSum;
            var bullPct = depthTotal > 0 ? (int)Math.Round((bidSum / depthTotal) * 100) : 50;

            // Foreign investor ratio vs total session volume
            var fBuyPct  = d.TotalVol > 0 ? Math.Round((d.FBuyVol  / d.TotalVol) * 100, 1) : 0.0;
            var fSellPct = d.TotalVol > 0 ? Math.Round((d.FSellVol / d.TotalVol) * 100, 1) : 0.0;

            // Max single-level depth volume (for bar width scaling)
            var maxDepthVol = Math.Max(1, new[] { d.AskVol1, d.AskVol2, d.AskVol3, d.BidVol1, d.BidVol2, d.BidVol3 }.Max());

            return new PriceDepthDto
            {
                Ticker = ticker,
                AskPrice1 = d.AskPrice1, AskVol1 = d.AskVol1,
                AskPrice2 = d.AskPrice2, AskVol2 = d.AskVol2,
                AskPrice3 = d.AskPrice3, AskVol3 = d.AskVol3,
                BidPrice1 = d.BidPrice1, BidVol1 = d.BidVol1,
                BidPrice2 = d.BidPrice2, BidVol2 = d.BidVol2,
                BidPrice3 = d.BidPrice3, BidVol3 = d.BidVol3,
                ReferencePrice = refPrice,
                CeilingPrice = d.CeilingPrice,
                FloorPrice = d.FloorPrice,
                Change = d.Change,
                RatioChange = d.RatioChange,
                TotalVol = d.TotalVol,
                AskChange1 = Chg(d.AskPrice1), AskChangePct1 = ChgPct(d.AskPrice1),
                AskChange2 = Chg(d.AskPrice2), AskChangePct2 = ChgPct(d.AskPrice2),
                AskChange3 = Chg(d.AskPrice3), AskChangePct3 = ChgPct(d.AskPrice3),
                BidChange1 = Chg(d.BidPrice1), BidChangePct1 = ChgPct(d.BidPrice1),
                BidChange2 = Chg(d.BidPrice2), BidChangePct2 = ChgPct(d.BidPrice2),
                BidChange3 = Chg(d.BidPrice3), BidChangePct3 = ChgPct(d.BidPrice3),
                FBuyVol = d.FBuyVol, FSellVol = d.FSellVol,
                FBuyVal = d.FBuyVal, FSellVal = d.FSellVal,
                TotalBuyVol = d.TotalBuyVol,
                TotalSellVol = d.TotalSellVol,
                BullPct = bullPct, BearPct = 100 - bullPct,
                FBuyPct = fBuyPct, FSellPct = fSellPct,
                MaxDepthVol = maxDepthVol,
                Side = d.Side,
                TradingSession = d.TradingSession,
            };
        }

        /// <summary>
        /// Check if new value is different from existing value, and add to updates if changed.
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="updates">Dictionary of updated fields</param>
        /// <param name="fieldName">Name of field</param>
        /// <param name="newValue">New value of field</param>
        /// <param name="existingValue">Old value of field</param>
        private static void AddIfChanged<T>(Dictionary<string, object> updates, string fieldName, T? newValue, T existingValue) where T : struct
        {
            var valueToCompare = newValue ?? default;
            if (!EqualityComparer<T>.Default.Equals(valueToCompare, existingValue))
            {
                updates[fieldName] = valueToCompare;
            }
        }

        /// <summary>
        /// Check if new value is different from existing value, and add to updates if changed.
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="updates">Dictionary of updated fields</param>
        /// <param name="fieldName">Name of field</param>
        /// <param name="newValue">New value of field</param>
        /// <param name="existingValue">Old value of field</param>
        private static void AddIfChanged(Dictionary<string, object> updates, string fieldName, string? newValue, string existingValue)
        {
            var valueToCompare = newValue ?? string.Empty;
            if (!string.Equals(valueToCompare, existingValue, StringComparison.Ordinal))
            {
                updates[fieldName] = valueToCompare;
            }
        }
        
        #endregion Helper Methods

        #region Handle OHLCV (Channel B)

        /// <summary>
        /// Handle Channel B: Realtime OHLCV data from SSI
        /// NEW LOGIC: Update ALL timeframes directly from SSI data (no aggregation)
        /// Each timeframe maintains its own state in Redis, updated incrementally
        /// </summary>
        /// <param name="redis">Redis service</param>
        /// <param name="response">OHLCV data from SSI Channel B</param>
        private async Task HandleOhlcvData(IRedisService redis, OhlcvDataResponse? response)
        {
            try
            {
                if (response == null || string.IsNullOrWhiteSpace(response.Symbol))
                {
                    _logger.LogWarning("Received null or invalid OHLCV data");
                    return;
                }

                // TradingTime is optional - SSI may not always send it
                // We use DateTime.UtcNow instead of TradingTime for candle calculations
                var now = DateTime.UtcNow;
                var ticker = response.Symbol.ToUpper();

                // Extract OHLCV values from SSI
                var ohlcvUpdate = new OhlcvUpdateData(
                    Open: (decimal)(response.Open ?? 0),
                    High: (decimal)(response.High ?? 0),
                    Low: (decimal)(response.Low ?? 0),
                    Close: (decimal)(response.Close ?? 0),
                    Volume: (long)(response.Volume ?? 0),
                    TotalValue: (decimal)(response.Value ?? 0)
                );

                // Log incoming SSI data for specific tickers (debugging)
                // if (ticker == "FPT" || ticker == "VNM" || ticker == "CTG")
                // {
                //     _logger.LogInformation(
                //         "📥 SSI Channel B received: {Ticker} @ {TradingTime} | O:{Open} H:{High} L:{Low} C:{Close} V:{Volume}",
                //         ticker, response.TradingTime ?? "N/A", ohlcvUpdate.Open, ohlcvUpdate.High, ohlcvUpdate.Low, ohlcvUpdate.Close, ohlcvUpdate.Volume);
                // }

                // Update ALL 9 timeframes directly from SSI data
                var timeframes = new[] { "M1", "M5", "M15", "M30", "H1", "H4", "D1", "W1", "MN1" };

                foreach (var timeframe in timeframes)
                {
                    await UpdateTimeframeCandle(redis, ticker, timeframe, ohlcvUpdate, now);
                }

                _logger.LogDebug(
                    "📊 SSI Update: {Ticker} | O:{Open} H:{High} L:{Low} C:{Close} V:{Volume}",
                    ticker, ohlcvUpdate.Open, ohlcvUpdate.High, ohlcvUpdate.Low, ohlcvUpdate.Close, ohlcvUpdate.Volume);
            }
            catch (TaskCanceledException)
            {
                // Task canceled during shutdown - this is expected, ignore silently
                return;
            }
            catch (OperationCanceledException)
            {
                // Operation canceled during shutdown - this is expected, ignore silently
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling OHLCV data for symbol: {Symbol}", response?.Symbol);
            }
        }

        /// <summary>
        /// Update a specific timeframe candle with new SSI data
        /// Logic: Get from Redis → Check new period → Update incremental → Save Redis → Broadcast
        /// </summary>
        private async Task UpdateTimeframeCandle(
            IRedisService redis,
            string ticker,
            string timeframe,
            OhlcvUpdateData ohlcvData,
            DateTime now)
        {
            try
            {
                string redisKey = $"OHLCV:{ticker}:{timeframe}";

                var semaphore = _redisLocks.GetOrAdd(redisKey, _ => new SemaphoreSlim(1, 1));
                
                if (!await semaphore.WaitAsync(TimeSpan.FromSeconds(5)))
                {
                    // _logger.LogWarning("Timeout waiting for lock {RedisKey}, skipping update", redisKey);
                    return;
                }

                try
                {
                    var existingCandle = await redis.GetAsync<CurrentCandleDto>(redisKey);

                var periodStart = GetPeriodStartTime(now, timeframe);

                CurrentCandleDto candle;

                if (existingCandle == null)
                {
                    // Only initialise candle state during trading hours.
                    // SSI sends a full-snapshot replay on every reconnect; creating a new candle
                    // from that replay outside trading hours produces phantom records (e.g. a
                    // Saturday D1 candle when the server restarts on a weekend).
                    if (!IsWithinTradingHours(now))
                        return;

                    candle = new CurrentCandleDto
                    {
                        Ticker = ticker,
                        Timeframe = timeframe,
                        StartTime = periodStart,
                        Open = ohlcvData.Close,
                        High = ohlcvData.Close,
                        Low = ohlcvData.Close,
                        Close = ohlcvData.Close,
                        Volume = ohlcvData.Volume,
                        TotalValue = ohlcvData.TotalValue,
                        LastUpdateTime = now,
                        IsComplete = false
                    };

                    // _logger.LogInformation(
                    //     "🆕 New {Timeframe} candle: {Ticker} @ {StartTime}",
                    //     timeframe, ticker, periodStart);
                }
                else if (existingCandle.StartTime < periodStart)
                {
                    // Only roll to a new candle period during trading hours.
                    // On server restart outside trading hours, SSI replays the last known snapshot.
                    // The replay's timestamp puts us in a "new" period relative to the stored candle,
                    // which would save the old candle to DB and create a ghost candle (e.g. a blank
                    // Saturday D1/M1 candle at restart time). Skip the roll entirely when idle.
                    if (!IsWithinTradingHours(now))
                        return;

                    if (ShouldSaveCompletedCandle(timeframe, existingCandle, now))
                    {
                        _saveCandleChannel.Writer.TryWrite(existingCandle);
                    }

                    candle = new CurrentCandleDto
                    {
                        Ticker = ticker,
                        Timeframe = timeframe,
                        StartTime = periodStart,
                        Open = ohlcvData.Close,
                        High = ohlcvData.Close,
                        Low = ohlcvData.Close,
                        Close = ohlcvData.Close,
                        Volume = ohlcvData.Volume,
                        TotalValue = ohlcvData.TotalValue,
                        LastUpdateTime = now,
                        IsComplete = false
                    };

                    // _logger.LogInformation(
                    //     "🆕 New {Timeframe} candle: {Ticker} @ {StartTime}",
                    //     timeframe, ticker, periodStart);
                }
                else
                {
                    candle = existingCandle;
                    candle.High = Math.Max(candle.High, ohlcvData.Close);
                    candle.Low = candle.Low == 0 ? ohlcvData.Close : Math.Min(candle.Low, ohlcvData.Close);
                    candle.Close = ohlcvData.Close;
                    candle.Volume += ohlcvData.Volume;
                    candle.TotalValue += ohlcvData.TotalValue;
                    candle.LastUpdateTime = now;
                    
                    _logger.LogDebug(
                        "📈 Update {Timeframe}: {Ticker} | C:{Close} V+:{Volume} Total:{TotalVolume}",
                        timeframe, ticker, candle.Close, ohlcvData.Volume, candle.Volume);
                }

                var ttl = GetRedisTTL(timeframe);
                await redis.SetAsync(redisKey, candle, ttl);

                await SafeBroadcastAsync(
                    () => _broadcaster.BroadcastOhlcvUpdateAsync(candle),
                    $"OHLCV {timeframe} for {ticker}");
                }
                finally
                {
                    semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update {Timeframe} for {Ticker}", timeframe, ticker);
            }
        }

        // SaveCandleToDatabase removed — candles are now enqueued to _saveCandleChannel
        // and flushed in batches by ProcessCandleSaveQueueAsync.

        /// <summary>
        /// Calculate period start time for a timeframe
        /// Examples:
        /// - M1 at 9:37:25 → 9:37:00
        /// - M5 at 9:37:00 → 9:35:00
        /// - H1 at 10:45:00 → 10:00:00
        /// - D1 at any time → 00:00:00 today
        /// - W1 at any time → 00:00:00 Monday of current week
        /// - MN1 at any time → 00:00:00 1st of current month
        /// </summary>
        private static DateTime GetPeriodStartTime(DateTime now, string timeframe)
        {
            return timeframe switch
            {
                "M1" => new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc),
                "M5" => RoundDownToMinutes(now, 5),
                "M15" => RoundDownToMinutes(now, 15),
                "M30" => RoundDownToMinutes(now, 30),
                "H1" => new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc),
                "H4" => RoundDownToHours(now, 4),
                "D1" => new DateTime(now.AddHours(7).Year, now.AddHours(7).Month, now.AddHours(7).Day, 8, 0, 0, DateTimeKind.Utc),
                "W1" => GetWeekStart(now),
                "MN1" => new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                _ => now
            };
        }

        /// <summary>
        /// Round down to nearest interval minutes
        /// Example: 9:37 with interval 5 → 9:35
        /// </summary>
        private static DateTime RoundDownToMinutes(DateTime time, int intervalMinutes)
        {
            var totalMinutes = time.Hour * 60 + time.Minute;
            var roundedMinutes = (totalMinutes / intervalMinutes) * intervalMinutes;
            var hour = roundedMinutes / 60;
            var minute = roundedMinutes % 60;
            return new DateTime(time.Year, time.Month, time.Day, hour, minute, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// Round down to nearest interval hours
        /// Example: 13:45 with interval 4 → 12:00 (12pm is 4-hour aligned from midnight)
        /// </summary>
        private static DateTime RoundDownToHours(DateTime time, int intervalHours)
        {
            var roundedHour = (time.Hour / intervalHours) * intervalHours;
            return new DateTime(time.Year, time.Month, time.Day, roundedHour, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// Returns true when the given UTC instant falls within VN stock-market trading hours.
        /// VN market (UTC+7):
        ///   Morning  : 09:00 – 11:30  (Mon–Fri)
        ///   Afternoon: 13:00 – 15:15  (Mon–Fri, includes ATC)
        /// Outside these windows SSI only sends snapshot replays, not live ticks.
        /// </summary>
        private static bool IsWithinTradingHours(DateTime utcNow)
        {
            var vn = utcNow.AddHours(7); // UTC → UTC+7
            if (vn.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                return false;

            var t = vn.TimeOfDay;
            return (t >= new TimeSpan(9, 0, 0)  && t <= new TimeSpan(11, 30, 0))
                || (t >= new TimeSpan(13, 0, 0) && t <= new TimeSpan(15, 15, 0));
        }

        /// <summary>
        /// Get start of current week (Monday)
        /// </summary>
        private static DateTime GetWeekStart(DateTime time)
        {
            var daysSinceMonday = ((int)time.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return time.Date.AddDays(-daysSinceMonday);
        }

        /// <summary>
        /// Determine if a completed candle should be saved to database
        /// M1: Save every minute when candle completes
        /// D1: Save only after market close (14:50 VN time = 07:50 UTC)
        /// Others: Don't save (M5-H4, W1, MN1)
        /// </summary>
        private static bool ShouldSaveCompletedCandle(string timeframe, CurrentCandleDto completedCandle, DateTime now)
        {
            if (timeframe == "M1")
            {
                // M1: Always save when minute ends
                return true;
            }
            
            if (timeframe == "D1")
            {
                // ShouldSaveCompletedCandle is only reached when existingCandle.StartTime < periodStart,
                // meaning the streaming engine detected a new VN calendar day has started.
                // At that point the old D1 candle is definitively closed — always save it.
                return true;
            }
            
            // M5-H4, W1, MN1: Don't save to database
            return false;
        }

        /// <summary>
        /// Get Redis TTL for each timeframe
        /// Shorter timeframes have shorter TTL (more frequent updates)
        /// </summary>
        private static TimeSpan GetRedisTTL(string timeframe)
        {
            return timeframe switch
            {
                // M1 TTL phải đủ dài để sống qua khoảng nghỉ giữa 2 phiên:
                // - Giữa sáng/chiều: 11:30 → 13:00 = 1.5 giờ
                // - Qua đêm: 14:30 → 09:00 sáng hôm sau = ~18.5 giờ
                // → dùng 24h để đảm bảo tick đầu phiên tiếp theo
                //   luôn thấy candle cuối phiên trước và flush xuống DB.
                "M1" => TimeSpan.FromHours(24),
                "M5" => TimeSpan.FromMinutes(30),
                "M15" => TimeSpan.FromHours(1),
                "M30" => TimeSpan.FromHours(2),
                "H1" => TimeSpan.FromHours(4),
                "H4" => TimeSpan.FromHours(8),
                "D1" => TimeSpan.FromHours(24),
                "W1" => TimeSpan.FromDays(7),
                "MN1" => TimeSpan.FromDays(30),
                _ => TimeSpan.FromHours(1)
            };
        }

        #endregion Handle OHLCV (Channel B)

        #region Heatmap Realtime Broadcasting (Per Symbol)

        /// <summary>
        /// Update heatmap data in Redis (single source of truth)
        /// Similar to OHLCV pattern - calculate metrics once and store in Redis
        /// </summary>
        /// <param name="redis">Redis service</param>
        /// <param name="ticker">Symbol ticker</param>
        /// <param name="updates">Updated fields from SSI</param>
        /// <param name="existingData">Existing market data</param>
        /// <returns>True if heatmap changed and needs broadcast, false otherwise</returns>
        private async Task<bool> UpdateHeatmapRedisAsync(
            IRedisService redis,
            string ticker,
            Dictionary<string, object> updates,
            MarketSymbolDto existingData)
        {
            try
            {
                bool heatmapRelevantFieldsChanged = 
                    updates.ContainsKey(nameof(MarketSymbolDto.LastPrice)) ||
                    updates.ContainsKey(nameof(MarketSymbolDto.Change)) ||
                    updates.ContainsKey(nameof(MarketSymbolDto.RatioChange)) ||
                    updates.ContainsKey(nameof(MarketSymbolDto.TotalVol)) ||
                    updates.ContainsKey(nameof(MarketSymbolDto.TotalVal)) ||
                    updates.ContainsKey(nameof(MarketSymbolDto.CeilingPrice)) ||
                    updates.ContainsKey(nameof(MarketSymbolDto.FloorPrice));

                if (!heatmapRelevantFieldsChanged)
                {
                    return false;
                }
                var lastPrice = GetValue<double>(updates, nameof(MarketSymbolDto.LastPrice), existingData.LastPrice);
                var referencePrice = GetValue<double>(updates, nameof(MarketSymbolDto.ReferencePrice), existingData.ReferencePrice);
                var totalVol = (long)GetValue<double>(updates, nameof(MarketSymbolDto.TotalVol), existingData.TotalVol);
                var totalVal = GetValue<double>(updates, nameof(MarketSymbolDto.TotalVal), existingData.TotalVal);
                var change = GetValue<double>(updates, nameof(MarketSymbolDto.Change), existingData.Change);
                var ratioChange = GetValue<double>(updates, nameof(MarketSymbolDto.RatioChange), existingData.RatioChange);
                var ceilingPrice = GetValue<double>(updates, nameof(MarketSymbolDto.CeilingPrice), existingData.CeilingPrice);
                var floorPrice = GetValue<double>(updates, nameof(MarketSymbolDto.FloorPrice), existingData.FloorPrice);

                if (lastPrice == 0 || referencePrice == 0)
                    return false;

                if (!_symbolMetadataCache.TryGetValue(ticker.ToUpper(), out var metadata))
                {
                    _logger.LogWarning("Symbol metadata not found in cache for {Ticker}", ticker);
                    return false;
                }

                var changePercent = (decimal)ratioChange;
                var changeValue = (decimal)change;
                var colorType = CalculateColorType(
                    (decimal)lastPrice,
                    (decimal)referencePrice,
                    (decimal)ceilingPrice,
                    (decimal)floorPrice,
                    changePercent);

                var heatmapItem = new HeatmapItemDto
                {
                    Ticker = ticker,
                    CompanyName = metadata.CompanyName,
                    CurrentPrice = (decimal)lastPrice,
                    ChangePercent = changePercent,
                    ChangeValue = changeValue,
                    Volume = totalVol,
                    TotalValue = (decimal)totalVal,
                    MarketCap = null, // Not available in Symbol entity
                    Exchange = metadata.Exchange,
                    Sector = metadata.SectorId,
                    SectorName = metadata.SectorName,
                    ColorType = colorType,
                    LastUpdate = DateTime.UtcNow
                };

                var heatmapKey = $"HEATMAP:{ticker.ToUpper()}";
                await redis.SetAsync(heatmapKey, heatmapItem, TimeSpan.FromMinutes(10));

                _logger.LogDebug(
                    "Updated heatmap Redis: {Ticker} | Price:{Price} Change:{Change}% Vol:{Volume}",
                    ticker, lastPrice, changePercent, totalVol);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update heatmap Redis for {Ticker}", ticker);
                return false;
            }
        }

        /// <summary>
        /// Create initial heatmap Redis entry for new symbol
        /// </summary>
        private async Task CreateInitialHeatmapRedisAsync(
            IRedisService redis,
            string ticker,
            MarketSymbolDto marketData)
        {
            try
            {
                if (marketData.LastPrice == 0 || marketData.ReferencePrice == 0)
                    return;

                if (!_symbolMetadataCache.TryGetValue(ticker.ToUpper(), out var metadata))
                {
                    _logger.LogWarning("Symbol metadata not found in cache for {Ticker}", ticker);
                    return;
                }

                var changePercent = (decimal)marketData.RatioChange;
                var colorType = CalculateColorType(
                    (decimal)marketData.LastPrice,
                    (decimal)marketData.ReferencePrice,
                    (decimal)marketData.CeilingPrice,
                    (decimal)marketData.FloorPrice,
                    changePercent);

                var heatmapItem = new HeatmapItemDto
                {
                    Ticker = ticker,
                    CompanyName = metadata.CompanyName,
                    CurrentPrice = (decimal)marketData.LastPrice,
                    ChangePercent = changePercent,
                    ChangeValue = (decimal)marketData.Change,
                    Volume = (long)marketData.TotalVol,
                    TotalValue = (decimal)marketData.TotalVal,
                    MarketCap = null,
                    Exchange = metadata.Exchange,
                    Sector = metadata.SectorId,
                    SectorName = metadata.SectorName,
                    ColorType = colorType,
                    LastUpdate = DateTime.UtcNow
                };

                var heatmapKey = $"HEATMAP:{ticker.ToUpper()}";
                await redis.SetAsync(heatmapKey, heatmapItem, TimeSpan.FromMinutes(10));

                _logger.LogInformation(
                    "🆕 Created heatmap Redis: {Ticker} | Price:{Price}",
                    ticker, marketData.LastPrice);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create heatmap Redis for {Ticker}", ticker);
            }
        }

        /// <summary>
        /// Broadcast heatmap item to SignalR from Redis (no calculation needed)
        /// </summary>
        private async Task BroadcastHeatmapItemFromRedisAsync(IRedisService redis, string ticker)
        {
            try
            {
                var heatmapKey = $"HEATMAP:{ticker.ToUpper()}";
                var heatmapItem = await redis.GetAsync<HeatmapItemDto>(heatmapKey);
                
                if (heatmapItem != null)
                {
                    await SafeBroadcastAsync(
                        () => _broadcaster.BroadcastHeatmapItemAsync(heatmapItem),
                        $"heatmap item for {ticker}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast heatmap item for {Ticker}", ticker);
            }
        }

        /// <summary>
        /// Calculate color type for heatmap based on price changes
        /// </summary>
        private static string CalculateColorType(
            decimal currentPrice,
            decimal referencePrice,
            decimal ceilingPrice,
            decimal floorPrice,
            decimal changePercent)
        {
            // Ceiling or Floor
            if (currentPrice >= ceilingPrice && ceilingPrice > 0)
                return "ceiling";
            if (currentPrice <= floorPrice && floorPrice > 0)
                return "floor";

            // Based on change percentage
            return changePercent switch
            {
                >= 3.0m => "strong-up",
                >= 0.5m => "up",
                <= -3.0m => "strong-down",
                <= -0.5m => "down",
                _ => "neutral"
            };
        }

        /// <summary>
        /// Get value from updates dictionary or fallback to existing value
        /// </summary>
        private static T GetValue<T>(Dictionary<string, object> updates, string key, T existingValue)
        {
            if (updates.TryGetValue(key, out var value))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            return existingValue;
        }

        /// <summary>
        /// Symbol metadata cached from database to avoid repeated queries
        /// </summary>
        private class SymbolMetadata
        {
            public required string Ticker { get; set; }
            public required string CompanyName { get; set; }
            public required string Exchange { get; set; }
            public string? SectorId { get; set; }
            public string? SectorName { get; set; }
        }

        #endregion
    }
}
