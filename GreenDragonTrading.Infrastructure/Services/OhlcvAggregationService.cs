using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <summary>
    /// Service for aggregating real-time tick data into OHLCV candles
    /// Maintains in-memory candles and saves completed candles to database
    /// </summary>
    public class OhlcvAggregationService : IOhlcvAggregationService, IDisposable
    {
        private readonly ILogger<OhlcvAggregationService> _logger;
        private readonly IMarketDataBroadcaster _broadcaster;
        private readonly IServiceProvider _serviceProvider;

        // In-memory storage: Ticker → Timeframe → CurrentCandle
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, CurrentCandle>> _candles;

        // Per-ticker locks for fine-grained concurrency control
        // Allows parallel processing of different tickers while maintaining consistency per ticker
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _tickerLocks;

        // Global lock for operations that need to access all tickers (flush, check expired)
        private readonly SemaphoreSlim _globalLock = new(1, 1);

        private bool _disposed;

        // Supported timeframes for realtime aggregation
        // M1: Base timeframe, stored to DB
        // M5, M15, M30, H1, H4: Aggregated from M1 ticks in real-time
        // D1: Aggregated from ticks for current day (for realtime chart)
        // W1: Weekly candles (Monday-Sunday)
        // MN1: Monthly candles (1st-last day of month)
        private static readonly string[] Timeframes = { "M1", "M5", "M15", "M30", "H1", "H4", "D1", "W1", "MN1" };

        public OhlcvAggregationService(
            ILogger<OhlcvAggregationService> logger,
            IMarketDataBroadcaster broadcaster,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _broadcaster = broadcaster;
            _serviceProvider = serviceProvider;
            _candles = new ConcurrentDictionary<string, ConcurrentDictionary<string, CurrentCandle>>();
            _tickerLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Initializing OHLCV Aggregation Service...");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var ohlcvRepository = scope.ServiceProvider.GetRequiredService<IOhlcvRepository>();

                // Get all tickers
                var tickers = await unitOfWork.Symbols.GetAllTickersAsync(cancellationToken);

                int recoveredCandles = 0;
                foreach (var ticker in tickers)
                {
                    foreach (var timeframe in Timeframes)
                    {
                        // Try to recover last incomplete candle from database
                        var recovered = await TryRecoverCandleAsync(ticker, timeframe, ohlcvRepository, cancellationToken);
                        if (recovered) recoveredCandles++;
                    }
                }

                _logger.LogInformation(
                    "OHLCV Aggregation Service initialized. Recovered {Count} incomplete candles.", 
                    recoveredCandles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing OHLCV Aggregation Service");
            }
        }

        public async Task ProcessTickAsync(TickData tick, CancellationToken cancellationToken = default)
        {
            if (!IsValidTick(tick))
            {
                return;
            }

            // Use per-ticker lock for better concurrency
            var tickerLock = GetTickerLock(tick.Ticker);
            await tickerLock.WaitAsync(cancellationToken);
            try
            {
                foreach (var timeframe in Timeframes)
                {
                    // Get or create candle for this timeframe
                    var candle = GetOrCreateCandle(tick.Ticker, timeframe, tick.Timestamp);

                    // Update candle with new tick
                    candle.UpdateWithTick(tick.Price, tick.Volume, tick.Value, tick.Timestamp);

                    // Debug log for D1, H1, H4 updates
                    if (timeframe == "D1" || timeframe == "H1" || timeframe == "H4")
                    {
                        _logger.LogDebug(
                            "Updated {Timeframe} candle for {Ticker} - O:{Open} H:{High} L:{Low} C:{Close} V:{Volume}",
                            timeframe, tick.Ticker, candle.Open, candle.High, candle.Low, candle.Close, candle.Volume);
                    }

                    // Broadcast in-progress candle update (only if candle has data)
                    // This prevents broadcasting empty candles with all prices = 0
                    if (candle.Volume > 0)
                    {
                        await BroadcastCandleUpdateAsync(candle, isComplete: false, cancellationToken);
                    }

                    // Check if candle should be closed
                    if (candle.ShouldClose(tick.Timestamp))
                    {
                        await CloseCandleAsync(candle, cancellationToken);
                    }
                }
            }
            finally
            {
                tickerLock.Release();
            }
        }

        public CurrentCandleDto? GetCurrentCandle(string ticker, string timeframe)
        {
            if (_candles.TryGetValue(ticker.ToUpper(), out var tickerCandles))
            {
                if (tickerCandles.TryGetValue(timeframe.ToUpper(), out var candle))
                {
                    return candle.ToDto(isComplete: false);
                }
            }
            return null;
        }

        public Dictionary<string, CurrentCandleDto> GetAllCurrentCandles(string ticker)
        {
            var result = new Dictionary<string, CurrentCandleDto>();

            if (_candles.TryGetValue(ticker.ToUpper(), out var tickerCandles))
            {
                foreach (var kvp in tickerCandles)
                {
                    result[kvp.Key] = kvp.Value.ToDto(isComplete: false);
                }
            }

            return result;
        }

        public async Task FlushAllCandlesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Flushing all incomplete candles to database...");

            await _globalLock.WaitAsync(cancellationToken);
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var ohlcvRepository = scope.ServiceProvider.GetRequiredService<IOhlcvRepository>();

                var candlesToSave = new List<Ohlcv>();

                foreach (var tickerPair in _candles)
                {
                    foreach (var timeframePair in tickerPair.Value)
                    {
                        var candle = timeframePair.Value;
                        var entity = MapToOhlcvEntity(candle);
                        candlesToSave.Add(entity);
                    }
                }

                if (candlesToSave.Any())
                {
                    var savedCount = await ohlcvRepository.BulkUpsertAsync(candlesToSave, cancellationToken);
                    _logger.LogInformation("Flushed {Count} incomplete candles to database", savedCount);
                }

                _candles.Clear();
            }
            finally
            {
                _globalLock.Release();
            }
        }

        public async Task CheckExpiredCandlesAsync(CancellationToken cancellationToken = default)
        {
            var currentTime = DateTime.UtcNow;
            await _globalLock.WaitAsync(cancellationToken);
            try
            {
                var candlesToClose = new List<CurrentCandle>();

                // Check all candles to see if they should be closed
                foreach (var tickerPair in _candles)
                {
                    foreach (var timeframePair in tickerPair.Value)
                    {
                        var candle = timeframePair.Value;
                        if (candle.ShouldClose(currentTime))
                        {
                            candlesToClose.Add(candle);
                        }
                    }
                }

                // Close expired candles
                foreach (var candle in candlesToClose)
                {
                    await CloseCandleAsync(candle, cancellationToken);
                }

                if (candlesToClose.Count > 0)
                {
                    _logger.LogInformation(
                        "Closed {Count} expired candles at {Time}",
                        candlesToClose.Count, currentTime);
                }
            }
            finally
            {
                _globalLock.Release();
            }
        }

        #region Private Methods

        /// <summary>
        /// Get or create a lock for a specific ticker
        /// </summary>
        private SemaphoreSlim GetTickerLock(string ticker)
        {
            var tickerKey = ticker.ToUpper();
            return _tickerLocks.GetOrAdd(tickerKey, _ => new SemaphoreSlim(1, 1));
        }

        private bool IsValidTick(TickData tick)
        {
            if (string.IsNullOrWhiteSpace(tick.Ticker) || tick.Price <= 0)
            {
                return false;
            }

            // Convert UTC to Vietnam timezone (GMT+7)
            var vnTime = tick.Timestamp.AddHours(7);
            var hour = vnTime.Hour;
            var minute = vnTime.Minute;

            // Trading hours: 09:00 - 15:00 (or 15:15 on Friday)
            if (hour < 9 || hour > 15)
            {
                return false;
            }

            if (hour == 15 && minute > 15)
            {
                return false;
            }

            return true;
        }

        private CurrentCandle GetOrCreateCandle(string ticker, string timeframe, DateTime timestamp)
        {
            var tickerKey = ticker.ToUpper();
            var timeframeKey = timeframe.ToUpper();

            var tickerCandles = _candles.GetOrAdd(tickerKey, _ => new ConcurrentDictionary<string, CurrentCandle>());

            return tickerCandles.GetOrAdd(timeframeKey, _ =>
            {
                var startTime = GetCandleStartTime(timestamp, timeframe);
                var newCandle = new CurrentCandle
                {
                    Ticker = tickerKey,
                    Timeframe = timeframeKey,
                    StartTime = startTime,
                    LastUpdateTime = timestamp
                };

                _logger.LogDebug(
                    "Created new {Timeframe} candle for {Ticker} starting at {StartTime}",
                    timeframe, ticker, startTime);

                return newCandle;
            });
        }

        private async Task CloseCandleAsync(CurrentCandle candle, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Closing {Timeframe} candle for {Ticker} - O:{Open} H:{High} L:{Low} C:{Close} V:{Volume}",
                    candle.Timeframe, candle.Ticker, candle.Open, candle.High, candle.Low, candle.Close, candle.Volume);

                // Save to database ONLY for M1 and D1 (base timeframes)
                // M5, M15, M30, H1, H4 are computed on-demand, not stored
                using var scope = _serviceProvider.CreateScope();
                var redisService = scope.ServiceProvider.GetRequiredService<IRedisService>();

                if (candle.Timeframe == "M1" || candle.Timeframe == "D1")
                {
                    var ohlcvRepository = scope.ServiceProvider.GetRequiredService<IOhlcvRepository>();
                    var entity = MapToOhlcvEntity(candle);
                    await ohlcvRepository.BulkUpsertAsync(new[] { entity }, cancellationToken);
                    
                    _logger.LogDebug(
                        "Saved {Timeframe} candle for {Ticker} to database",
                        candle.Timeframe, candle.Ticker);
                }

                // Broadcast completed candle for ALL timeframes
                await BroadcastCandleUpdateAsync(candle, isComplete: true, cancellationToken);

                // Cache invalidation: ONLY for D1 (end of day), NOT for M1
                // M1 closes every minute for ~1500 tickers → would overload Redis
                // Cache has TTL and will expire naturally
                // D1 is important for daily aggregations, so invalidate cache at end of day
                if (candle.Timeframe == "D1")
                {
                    try
                    {
                        var cacheKeyPattern = RedisConstants.OhlcvPattern(candle.Ticker);
                        await redisService.DeleteByPatternAsync(cacheKeyPattern);
                        
                        _logger.LogInformation(
                            "Invalidated cache pattern {Pattern} after closing D1 candle for {Ticker}",
                            cacheKeyPattern, candle.Ticker);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, 
                            "Failed to invalidate cache for {Ticker} after closing D1 candle - cache will expire naturally",
                            candle.Ticker);
                    }
                }

                // Remove from memory
                if (_candles.TryGetValue(candle.Ticker, out var tickerCandles))
                {
                    tickerCandles.TryRemove(candle.Timeframe, out _);
                }

                _logger.LogDebug(
                    "Successfully closed {Timeframe} candle for {Ticker}",
                    candle.Timeframe, candle.Ticker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error closing {Timeframe} candle for {Ticker}",
                    candle.Timeframe, candle.Ticker);
            }
        }

        private async Task BroadcastCandleUpdateAsync(
            CurrentCandle candle, 
            bool isComplete, 
            CancellationToken cancellationToken)
        {
            try
            {
                // Broadcast update for this timeframe
                var dto = candle.ToDto(isComplete);
                await _broadcaster.BroadcastOhlcvUpdateAsync(dto, cancellationToken);
                
                _logger.LogDebug(
                    "Broadcasted {Status} {Timeframe} update for {Ticker}",
                    isComplete ? "completed" : "in-progress", candle.Timeframe, candle.Ticker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "❌ Error broadcasting {Timeframe} candle update for {Ticker}. Exception: {Message}",
                    candle.Timeframe, candle.Ticker, ex.Message);
                // Throw to see the real error
                throw;
            }
        }

        private Ohlcv MapToOhlcvEntity(CurrentCandle candle)
        {
            return new Ohlcv
            {
                Time = candle.StartTime,
                Ticker = candle.Ticker,
                Timeframe = candle.Timeframe,
                Open = candle.Open,
                High = candle.High,
                Low = candle.Low,
                Close = candle.Close,
                Volume = candle.Volume,
                Value = candle.TotalValue,
                Source = "REALTIME_AGGREGATION",
                IsPreliminary = candle.Timeframe == "D1",
                CreatedAt = DateTime.UtcNow
            };
        }

        private DateTime GetCandleStartTime(DateTime timestamp, string timeframe)
        {
            // Convert UTC to Vietnam timezone (GMT+7) for proper candle alignment
            var vnTime = timestamp.AddHours(7);
            
            var result = timeframe.ToUpper() switch
            {
                "M1" => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day,
                    timestamp.Hour, timestamp.Minute, 0, DateTimeKind.Utc),
                "M5" => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day,
                    timestamp.Hour, (timestamp.Minute / 5) * 5, 0, DateTimeKind.Utc),
                "M15" => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day,
                    timestamp.Hour, (timestamp.Minute / 15) * 15, 0, DateTimeKind.Utc),
                "M30" => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day,
                    timestamp.Hour, (timestamp.Minute / 30) * 30, 0, DateTimeKind.Utc),
                "H1" => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day,
                    timestamp.Hour, 0, 0, DateTimeKind.Utc),
                "H4" => GetH4CandleStartTime(vnTime), // Use VN time for H4 alignment
                "D1" => GetDailyCandleStartTime(vnTime), // Use VN time for D1 alignment
                "W1" => GetWeeklyCandleStartTime(vnTime), // Weekly: Monday 00:00 VN time
                "MN1" => GetMonthlyCandleStartTime(vnTime), // Monthly: 1st day 00:00 VN time
                _ => throw new ArgumentException($"Invalid timeframe: {timeframe}")
            };
            
            return result;
        }
        
        /// <summary>
        /// Get H4 candle start time aligned with Vietnam trading hours
        /// Vietnam market: 09:00-15:00 (GMT+7)
        /// H4 candles: 09:00-13:00, 13:00-17:00 (covers full trading day + after hours)
        /// </summary>
        private DateTime GetH4CandleStartTime(DateTime vnTime)
        {
            var hour = vnTime.Hour;
            
            // Determine which H4 period this time belongs to
            // 09:00-13:00, 13:00-17:00, 17:00-21:00, 21:00-01:00, 01:00-05:00, 05:00-09:00
            var startHour = hour switch
            {
                >= 9 and < 13 => 9,
                >= 13 and < 17 => 13,
                >= 17 and < 21 => 17,
                >= 21 => 21,
                >= 1 and < 5 => 1,
                >= 5 and < 9 => 5,
                _ => 1 // 00:00-01:00 -> previous period starting at 21:00 previous day
            };
            
            // Handle midnight rollover
            if (hour >= 0 && hour < 1)
            {
                // 00:00-01:00 belongs to 21:00-01:00 period
                startHour = 21;
                vnTime = vnTime.AddDays(-1);
            }
            
            var candleStartVn = new DateTime(
                vnTime.Year, vnTime.Month, vnTime.Day,
                startHour, 0, 0, DateTimeKind.Unspecified);
            
            // Convert back to UTC (subtract 7 hours) and specify UTC kind
            var candleStartUtc = candleStartVn.AddHours(-7);
            return DateTime.SpecifyKind(candleStartUtc, DateTimeKind.Utc);
        }
        
        /// <summary>
        /// Get D1 candle start time
        /// D1 candles start at 00:00 Vietnam time (17:00 previous day UTC)
        /// This aligns with Vietnam calendar day
        /// </summary>
        private DateTime GetDailyCandleStartTime(DateTime vnTime)
        {
            // D1 candle uses Vietnam calendar day starting at 00:00 VN time
            var vnDate = vnTime.Date;
            var candleStartVn = new DateTime(vnDate.Year, vnDate.Month, vnDate.Day, 0, 0, 0, DateTimeKind.Unspecified);
            
            // Convert back to UTC (subtract 7 hours) and specify UTC kind
            // VN 00:00 = UTC 17:00 previous day
            var candleStartUtc = candleStartVn.AddHours(-7);
            return DateTime.SpecifyKind(candleStartUtc, DateTimeKind.Utc);
        }
        
        /// <summary>
        /// Get W1 candle start time
        /// Weekly candles start on Monday 00:00 Vietnam time
        /// </summary>
        private DateTime GetWeeklyCandleStartTime(DateTime vnTime)
        {
            // Get the start of the week (Monday 00:00 VN time)
            var daysFromMonday = ((int)vnTime.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var mondayVn = vnTime.Date.AddDays(-daysFromMonday);
            var candleStartVn = new DateTime(mondayVn.Year, mondayVn.Month, mondayVn.Day, 0, 0, 0, DateTimeKind.Unspecified);
            
            // Convert back to UTC (subtract 7 hours)
            var candleStartUtc = candleStartVn.AddHours(-7);
            return DateTime.SpecifyKind(candleStartUtc, DateTimeKind.Utc);
        }
        
        /// <summary>
        /// Get MN1 candle start time
        /// Monthly candles start on 1st day of month at 00:00 Vietnam time
        /// </summary>
        private DateTime GetMonthlyCandleStartTime(DateTime vnTime)
        {
            // Get the 1st day of the month at 00:00 VN time
            var firstDayVn = new DateTime(vnTime.Year, vnTime.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
            
            // Convert back to UTC (subtract 7 hours)
            var candleStartUtc = firstDayVn.AddHours(-7);
            return DateTime.SpecifyKind(candleStartUtc, DateTimeKind.Utc);
        }

        private async Task<bool> TryRecoverCandleAsync(
            string ticker, 
            string timeframe, 
            IOhlcvRepository ohlcvRepository,
            CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTime.UtcNow;
                var startTime = GetCandleStartTime(now, timeframe);

                // Query last candle from database
                var lastCandles = await ohlcvRepository.GetByTickerAndTimeRangeAsync(
                    ticker, timeframe, startTime, now, cancellationToken);

                var lastCandle = lastCandles.OrderByDescending(c => c.Time).FirstOrDefault();
                if (lastCandle != null && lastCandle.Time == startTime)
                {
                    // Candle exists and is still current - recover it
                    var currentCandle = new CurrentCandle
                    {
                        Ticker = ticker.ToUpper(),
                        Timeframe = timeframe.ToUpper(),
                        StartTime = lastCandle.Time,
                        Open = lastCandle.Open,
                        High = lastCandle.High,
                        Low = lastCandle.Low,
                        Close = lastCandle.Close,
                        Volume = lastCandle.Volume,
                        TotalValue = lastCandle.Value ?? 0,
                        LastUpdateTime = lastCandle.CreatedAt
                    };

                    var tickerCandles = _candles.GetOrAdd(
                        ticker.ToUpper(), 
                        _ => new ConcurrentDictionary<string, CurrentCandle>());
                    
                    tickerCandles[timeframe.ToUpper()] = currentCandle;

                    _logger.LogDebug(
                        "Recovered {Timeframe} candle for {Ticker} from database",
                        timeframe, ticker);

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, 
                    "Error recovering {Timeframe} candle for {Ticker}",
                    timeframe, ticker);
            }

            return false;
        }

        /// <summary>
        /// Dispose resources including all semaphore locks
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _globalLock?.Dispose();

            // Dispose all per-ticker locks
            foreach (var lockItem in _tickerLocks.Values)
            {
                try
                {
                    lockItem?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing ticker lock");
                }
            }
            _tickerLocks.Clear();

            _disposed = true;
        }

        #endregion
    }
}
