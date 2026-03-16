using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    public partial class SsiStreamingBackgroundService
    {
        private readonly object _batchLock = new();

        // 100ms batching buffers.
        // For hash/string writes and key-based broadcasts, latest value wins within each batch window.
        private readonly Dictionary<string, Dictionary<string, object>> _pendingRedisHashWrites = new(StringComparer.Ordinal);
        private readonly Dictionary<string, RedisStringWrite> _pendingRedisStringWrites = new(StringComparer.Ordinal);
        private readonly List<RedisListPushTrimWrite> _pendingRedisListWrites = [];

        private readonly Dictionary<string, Dictionary<string, object>> _pendingMarketBroadcasts = new(StringComparer.Ordinal);
        private readonly Dictionary<string, PriceDepthDto> _pendingDepthBroadcasts = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HeatmapItemDto> _pendingHeatmapBroadcasts = new(StringComparer.Ordinal);
        private readonly Dictionary<string, CurrentCandleDto> _pendingOhlcvBroadcasts = new(StringComparer.Ordinal);
        private readonly List<RecentTradeDto> _pendingTradeBroadcasts = [];

        private static readonly TimeSpan BatchInterval = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Flushes queued Redis writes every 100ms.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token for graceful shutdown.</param>
        private async Task ProcessRedisWriteBatchAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Redis write batch loop started (interval: {Ms}ms).", BatchInterval.TotalMilliseconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(BatchInterval, stoppingToken);

                    Dictionary<string, Dictionary<string, object>> hashWrites;
                    Dictionary<string, RedisStringWrite> stringWrites;
                    List<RedisListPushTrimWrite> listWrites;

                    lock (_batchLock)
                    {
                        if (_pendingRedisHashWrites.Count == 0
                            && _pendingRedisStringWrites.Count == 0
                            && _pendingRedisListWrites.Count == 0)
                        {
                            continue;
                        }

                        hashWrites = _pendingRedisHashWrites.ToDictionary(
                            kvp => kvp.Key,
                            kvp => new Dictionary<string, object>(kvp.Value),
                            StringComparer.Ordinal);
                        stringWrites = new Dictionary<string, RedisStringWrite>(_pendingRedisStringWrites, StringComparer.Ordinal);
                        listWrites = [.. _pendingRedisListWrites];

                        _pendingRedisHashWrites.Clear();
                        _pendingRedisStringWrites.Clear();
                        _pendingRedisListWrites.Clear();
                    }

                    using var scope = _serviceScopeFactory.CreateScope();
                    var redis = scope.ServiceProvider.GetRequiredService<IRedisService>();

                    if (redis is RedisService redisService)
                    {
                        // Fast path: one pipelined execution for all batched writes.
                        var hashBatch = hashWrites.Select(x => new RedisHashFieldsWrite(x.Key, x.Value)).ToList();
                        var stringBatch = stringWrites.Values.ToList();
                        await redisService.ExecuteBatchedWritesAsync(hashBatch, stringBatch, listWrites);
                    }
                    else
                    {
                        // Fallback path for alternate IRedisService implementations.
                        foreach (var write in hashWrites)
                        {
                            await redis.SetHashFieldsAsync(write.Key, write.Value);
                        }

                        foreach (var write in stringWrites.Values)
                        {
                            await redis.SetAsync(write.Key, write.Value, write.Expiry);
                        }

                        foreach (var write in listWrites)
                        {
                            await redis.ListPushTrimAsync(write.Key, write.Value, write.MaxLength);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Redis write batch loop.");
                }
            }

            _logger.LogInformation("Redis write batch loop stopped.");
        }

        /// <summary>
        /// Flushes queued SignalR broadcasts every 100ms while preserving current frontend event contracts.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token for graceful shutdown.</param>
        private async Task ProcessBroadcastBatchAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Broadcast batch loop started (interval: {Ms}ms).", BatchInterval.TotalMilliseconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(BatchInterval, stoppingToken);

                    Dictionary<string, Dictionary<string, object>> marketBatch;
                    Dictionary<string, PriceDepthDto> depthBatch;
                    Dictionary<string, HeatmapItemDto> heatmapBatch;
                    Dictionary<string, CurrentCandleDto> ohlcvBatch;
                    List<RecentTradeDto> tradeBatch;

                    lock (_batchLock)
                    {
                        if (_pendingMarketBroadcasts.Count == 0
                            && _pendingDepthBroadcasts.Count == 0
                            && _pendingHeatmapBroadcasts.Count == 0
                            && _pendingOhlcvBroadcasts.Count == 0
                            && _pendingTradeBroadcasts.Count == 0)
                        {
                            continue;
                        }

                        marketBatch = _pendingMarketBroadcasts.ToDictionary(
                            kvp => kvp.Key,
                            kvp => new Dictionary<string, object>(kvp.Value),
                            StringComparer.Ordinal);
                        depthBatch = new Dictionary<string, PriceDepthDto>(_pendingDepthBroadcasts, StringComparer.Ordinal);
                        heatmapBatch = new Dictionary<string, HeatmapItemDto>(_pendingHeatmapBroadcasts, StringComparer.Ordinal);
                        ohlcvBatch = new Dictionary<string, CurrentCandleDto>(_pendingOhlcvBroadcasts, StringComparer.Ordinal);
                        tradeBatch = [.. _pendingTradeBroadcasts];

                        _pendingMarketBroadcasts.Clear();
                        _pendingDepthBroadcasts.Clear();
                        _pendingHeatmapBroadcasts.Clear();
                        _pendingOhlcvBroadcasts.Clear();
                        _pendingTradeBroadcasts.Clear();
                    }

                    foreach (var item in marketBatch)
                    {
                        await SafeBroadcastAsync(
                            () => _broadcaster.BroadcastMarketDataAsync(item.Key, item.Value),
                            $"market data for {item.Key}");
                    }

                    foreach (var item in depthBatch.Values)
                    {
                        await SafeBroadcastAsync(
                            () => _broadcaster.BroadcastPriceDepthAsync(item),
                            $"price depth for {item.Ticker}");
                    }

                    foreach (var item in heatmapBatch.Values)
                    {
                        await SafeBroadcastAsync(
                            () => _broadcaster.BroadcastHeatmapItemAsync(item),
                            $"heatmap item for {item.Ticker}");
                    }

                    foreach (var item in ohlcvBatch.Values)
                    {
                        await SafeBroadcastAsync(
                            () => _broadcaster.BroadcastOhlcvUpdateAsync(item),
                            $"OHLCV {item.Timeframe} for {item.Ticker}");
                    }

                    foreach (var item in tradeBatch)
                    {
                        await SafeBroadcastAsync(
                            () => _broadcaster.BroadcastTradeAsync(item),
                            $"trade for {item.Ticker}");
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in broadcast batch loop.");
                }
            }

            _logger.LogInformation("Broadcast batch loop stopped.");
        }

        /// <summary>
        /// Executes one broadcast operation without letting exceptions interrupt the processing loops.
        /// </summary>
        /// <param name="broadcastAction">Broadcast action to execute.</param>
        /// <param name="context">Short context used for warning logs.</param>
        private async Task SafeBroadcastAsync(Func<Task> broadcastAction, string context)
        {
            try
            {
                await broadcastAction();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Broadcast failed for {Context}. Data saved but clients not notified.", context);
            }
        }

        /// <summary>
        /// Queues hash field updates and merges fields by key in the current batch window.
        /// </summary>
        /// <param name="key">Redis hash key.</param>
        /// <param name="updates">Changed fields to upsert.</param>
        private void QueueRedisHashFieldsWrite(string key, Dictionary<string, object> updates)
        {
            if (updates.Count == 0)
            {
                return;
            }

            lock (_batchLock)
            {
                if (!_pendingRedisHashWrites.TryGetValue(key, out var current))
                {
                    _pendingRedisHashWrites[key] = new Dictionary<string, object>(updates);
                    return;
                }

                foreach (var entry in updates)
                {
                    current[entry.Key] = entry.Value;
                }
            }
        }

        /// <summary>
        /// Queues an entire market DTO as hash field writes.
        /// </summary>
        /// <param name="key">Redis hash key.</param>
        /// <param name="value">Market symbol snapshot.</param>
        private void QueueRedisHashObjectWrite(string key, MarketSymbolDto value)
        {
            var updates = ConvertMarketDataToDictionary(value);
            QueueRedisHashFieldsWrite(key, updates);
        }

        /// <summary>
        /// Queues a Redis string write; latest write for same key overrides earlier ones in current batch.
        /// </summary>
        /// <param name="key">Redis key.</param>
        /// <param name="value">Value to store.</param>
        /// <param name="expiry">Optional expiry.</param>
        private void QueueRedisStringWrite(string key, object value, TimeSpan? expiry = null)
        {
            lock (_batchLock)
            {
                _pendingRedisStringWrites[key] = new RedisStringWrite(key, value, expiry);
            }
        }

        /// <summary>
        /// Queues list push + trim operation to keep capped list length.
        /// </summary>
        /// <param name="key">Redis list key.</param>
        /// <param name="value">Item to push.</param>
        /// <param name="maxLength">Maximum list length after trimming.</param>
        private void QueueRedisListPushTrimWrite(string key, object value, int maxLength)
        {
            lock (_batchLock)
            {
                _pendingRedisListWrites.Add(new RedisListPushTrimWrite(key, value, maxLength));
            }
        }

        /// <summary>
        /// Queues market patch broadcast and merges fields per symbol during current batch window.
        /// </summary>
        /// <param name="symbol">Ticker symbol.</param>
        /// <param name="updates">Changed fields payload.</param>
        private void QueueMarketBroadcast(string symbol, Dictionary<string, object> updates)
        {
            lock (_batchLock)
            {
                if (!_pendingMarketBroadcasts.TryGetValue(symbol, out var current))
                {
                    _pendingMarketBroadcasts[symbol] = new Dictionary<string, object>(updates);
                    return;
                }

                foreach (var entry in updates)
                {
                    current[entry.Key] = entry.Value;
                }
            }
        }

        /// <summary>
        /// Queues market broadcast from full DTO by converting it to dictionary payload.
        /// </summary>
        /// <param name="symbol">Ticker symbol.</param>
        /// <param name="dto">Market symbol snapshot.</param>
        private void QueueMarketBroadcast(string symbol, MarketSymbolDto dto)
        {
            var updates = ConvertMarketDataToDictionary(dto);
            updates["Ticker"] = symbol;
            QueueMarketBroadcast(symbol, updates);
        }

        /// <summary>
        /// Queues latest price-depth snapshot per ticker for broadcasting.
        /// </summary>
        /// <param name="depth">Price-depth payload.</param>
        private void QueuePriceDepthBroadcast(PriceDepthDto depth)
        {
            lock (_batchLock)
            {
                _pendingDepthBroadcasts[depth.Ticker.ToUpper()] = depth;
            }
        }

        /// <summary>
        /// Queues latest heatmap payload per ticker for broadcasting.
        /// </summary>
        /// <param name="item">Heatmap item payload.</param>
        private void QueueHeatmapBroadcast(HeatmapItemDto item)
        {
            lock (_batchLock)
            {
                _pendingHeatmapBroadcasts[item.Ticker.ToUpper()] = item;
            }
        }

        /// <summary>
        /// Queues latest OHLCV candle update per ticker/timeframe key.
        /// </summary>
        /// <param name="candle">OHLCV candle payload.</param>
        private void QueueOhlcvBroadcast(CurrentCandleDto candle)
        {
            var key = $"{candle.Ticker}:{candle.Timeframe}";
            lock (_batchLock)
            {
                _pendingOhlcvBroadcasts[key] = candle;
            }
        }

        /// <summary>
        /// Queues a trade tape item; list order is preserved within batch window.
        /// </summary>
        /// <param name="trade">Recent trade payload.</param>
        private void QueueTradeBroadcast(RecentTradeDto trade)
        {
            lock (_batchLock)
            {
                _pendingTradeBroadcasts.Add(trade);
            }
        }

        /// <summary>
        /// Creates a deep clone of market snapshot using JSON serialization.
        /// </summary>
        /// <param name="source">Source market snapshot.</param>
        /// <returns>Cloned market snapshot.</returns>
        private static MarketSymbolDto CloneMarketData(MarketSymbolDto source)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(source);
            return System.Text.Json.JsonSerializer.Deserialize<MarketSymbolDto>(json) ?? new MarketSymbolDto();
        }

        /// <summary>
        /// Converts non-null properties of market snapshot into field dictionary.
        /// </summary>
        /// <param name="dto">Market symbol snapshot.</param>
        /// <returns>Field dictionary for patch/update operations.</returns>
        private static Dictionary<string, object> ConvertMarketDataToDictionary(MarketSymbolDto dto)
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal);
            var props = typeof(MarketSymbolDto).GetProperties();

            foreach (var prop in props)
            {
                var value = prop.GetValue(dto);
                if (value != null)
                {
                    result[prop.Name] = value;
                }
            }

            return result;
        }

        /// <summary>
        /// Applies partial field updates to object properties by matching property names.
        /// </summary>
        /// <typeparam name="T">Target object type.</typeparam>
        /// <param name="target">Target object to mutate.</param>
        /// <param name="updates">Field updates map.</param>
        private static void ApplyFieldUpdatesToObject<T>(T target, Dictionary<string, object> updates)
        {
            var targetType = typeof(T);

            foreach (var kvp in updates)
            {
                var prop = targetType.GetProperty(kvp.Key);
                if (prop == null || !prop.CanWrite)
                {
                    continue;
                }

                var value = kvp.Value;
                if (value == null)
                {
                    continue;
                }

                var targetPropType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                var converted = Convert.ChangeType(value, targetPropType);
                prop.SetValue(target, converted);
            }
        }
    }
}
