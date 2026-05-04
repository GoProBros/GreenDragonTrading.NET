using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    public partial class SsiStreamingBackgroundService
    {
        // Symbol metadata cache to avoid DB queries on every tick.
        // Key is always upper-cased ticker.
        private readonly ConcurrentDictionary<string, SymbolMetadata> _symbolMetadataCache = new();

        /// <summary>
        /// Initializes in-memory symbol metadata cache from database.
        /// </summary>
        /// <param name="uow">Unit of work for symbol queries.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task InitializeSymbolCacheAsync(IUnitOfWork uow, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Initializing symbol metadata cache...");

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
                        SectorName = symbol.Sector?.ViName ?? symbol.Sector?.EnName,
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
        /// Updates heatmap projection in Redis and queues heatmap broadcast when relevant fields changed.
        /// </summary>
        /// <param name="redis">Redis service abstraction.</param>
        /// <param name="ticker">Ticker symbol.</param>
        /// <param name="updates">Changed market fields from channel handlers.</param>
        /// <param name="existingData">Existing market snapshot for fallback values.</param>
        /// <returns>True if heatmap payload was updated and queued.</returns>
        private async Task<bool> UpdateHeatmapRedisAsync(
            IRedisService redis,
            string ticker,
            Dictionary<string, object> updates,
            MarketSymbolDto existingData)
        {
            try
            {
                // Only recompute heatmap when one of these fields changed.
                var heatmapRelevantFieldsChanged =
                    updates.ContainsKey(nameof(MarketSymbolDto.LastPrice))
                    || updates.ContainsKey(nameof(MarketSymbolDto.Change))
                    || updates.ContainsKey(nameof(MarketSymbolDto.RatioChange))
                    || updates.ContainsKey(nameof(MarketSymbolDto.TotalVol))
                    || updates.ContainsKey(nameof(MarketSymbolDto.TotalVal))
                    || updates.ContainsKey(nameof(MarketSymbolDto.CeilingPrice))
                    || updates.ContainsKey(nameof(MarketSymbolDto.FloorPrice));

                if (!heatmapRelevantFieldsChanged)
                {
                    return false;
                }

                // Merge changed fields with existing snapshot to calculate current heatmap state.
                var lastPrice = GetValue(updates, nameof(MarketSymbolDto.LastPrice), existingData.LastPrice);
                var referencePrice = GetValue(updates, nameof(MarketSymbolDto.ReferencePrice), existingData.ReferencePrice);
                var totalVol = (long)GetValue(updates, nameof(MarketSymbolDto.TotalVol), existingData.TotalVol);
                var totalVal = GetValue(updates, nameof(MarketSymbolDto.TotalVal), existingData.TotalVal);
                var change = GetValue(updates, nameof(MarketSymbolDto.Change), existingData.Change);
                var ratioChange = GetValue(updates, nameof(MarketSymbolDto.RatioChange), existingData.RatioChange);
                var ceilingPrice = GetValue(updates, nameof(MarketSymbolDto.CeilingPrice), existingData.CeilingPrice);
                var floorPrice = GetValue(updates, nameof(MarketSymbolDto.FloorPrice), existingData.FloorPrice);

                if (lastPrice == 0 || referencePrice == 0)
                {
                    // Heatmap color/value logic needs both prices to be meaningful.
                    return false;
                }

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
                    MarketCap = null,
                    Exchange = metadata.Exchange,
                    Sector = metadata.SectorId,
                    SectorName = metadata.SectorName,
                    ColorType = colorType,
                    LastUpdate = DateTime.UtcNow,
                };

                var heatmapKey = RedisConstants.Heatmap(ticker);
                QueueRedisStringWrite(heatmapKey, heatmapItem);
                QueueHeatmapBroadcast(heatmapItem);

                _logger.LogDebug(
                    "Updated heatmap Redis: {Ticker} | Price:{Price} Change:{Change}% Vol:{Volume}",
                    ticker,
                    lastPrice,
                    changePercent,
                    totalVol);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update heatmap Redis for {Ticker}", ticker);
                return false;
            }
        }

        /// <summary>
        /// Creates initial heatmap Redis entry for new symbol market snapshot.
        /// </summary>
        /// <param name="redis">Redis service abstraction.</param>
        /// <param name="ticker">Ticker symbol.</param>
        /// <param name="marketData">Current market data snapshot.</param>
        private async Task CreateInitialHeatmapRedisAsync(
            IRedisService redis,
            string ticker,
            MarketSymbolDto marketData)
        {
            try
            {
                if (marketData.LastPrice == 0 || marketData.ReferencePrice == 0)
                {
                    return;
                }

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
                    LastUpdate = DateTime.UtcNow,
                };

                var heatmapKey = RedisConstants.Heatmap(ticker);
                QueueRedisStringWrite(heatmapKey, heatmapItem);
                QueueHeatmapBroadcast(heatmapItem);

                _logger.LogInformation(
                    "Created heatmap Redis: {Ticker} | Price:{Price}",
                    ticker,
                    marketData.LastPrice);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create heatmap Redis for {Ticker}", ticker);
            }
        }

        /// <summary>
        /// Reads heatmap item from Redis and queues it for SignalR broadcast.
        /// </summary>
        /// <param name="redis">Redis service abstraction.</param>
        /// <param name="ticker">Ticker symbol.</param>
        private async Task BroadcastHeatmapItemFromRedisAsync(IRedisService redis, string ticker)
        {
            try
            {
                var heatmapKey = RedisConstants.Heatmap(ticker);
                var heatmapItem = await redis.GetAsync<HeatmapItemDto>(heatmapKey);

                if (heatmapItem != null)
                {
                    QueueHeatmapBroadcast(heatmapItem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast heatmap item for {Ticker}", ticker);
            }
        }

        /// <summary>
        /// Calculates heatmap color bucket based on ceiling/floor and change percent.
        /// </summary>
        /// <param name="currentPrice">Current traded price.</param>
        /// <param name="referencePrice">Reference price.</param>
        /// <param name="ceilingPrice">Ceiling price.</param>
        /// <param name="floorPrice">Floor price.</param>
        /// <param name="changePercent">Change percent.</param>
        /// <returns>Color bucket key used by frontend.</returns>
        private static string CalculateColorType(
            decimal currentPrice,
            decimal referencePrice,
            decimal ceilingPrice,
            decimal floorPrice,
            decimal changePercent)
        {
            if (currentPrice >= ceilingPrice && ceilingPrice > 0)
            {
                return "ceiling";
            }

            if (currentPrice <= floorPrice && floorPrice > 0)
            {
                return "floor";
            }

            return changePercent switch
            {
                >= 3.0m => "strong-up",
                >= 0.5m => "up",
                <= -3.0m => "strong-down",
                <= -0.5m => "down",
                _ => "neutral",
            };
        }

        /// <summary>
        /// Reads value from update map or falls back to existing value.
        /// </summary>
        /// <typeparam name="T">Target value type.</typeparam>
        /// <param name="updates">Changed fields map.</param>
        /// <param name="key">Field key to lookup.</param>
        /// <param name="existingValue">Fallback value when key is absent.</param>
        /// <returns>Updated or fallback value.</returns>
        private static T GetValue<T>(Dictionary<string, object> updates, string key, T existingValue)
        {
            if (updates.TryGetValue(key, out var value))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }

            return existingValue;
        }

        /// <summary>
        /// Symbol metadata cached from database to avoid repeated lookups.
        /// </summary>
        private class SymbolMetadata
        {
            public required string Ticker { get; set; }

            public required string CompanyName { get; set; }

            public required string Exchange { get; set; }

            public string? SectorId { get; set; }

            public string? SectorName { get; set; }
        }
    }
}
