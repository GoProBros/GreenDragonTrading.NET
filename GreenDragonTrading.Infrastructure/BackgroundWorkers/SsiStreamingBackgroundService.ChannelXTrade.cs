using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    public partial class SsiStreamingBackgroundService
    {
        /// <summary>
        /// Handles X-TRADE channel messages and updates market/trade state in Redis + broadcast queues.
        /// </summary>
        /// <param name="redis">Redis service abstraction.</param>
        /// <param name="response">Deserialized X-TRADE payload from SSI.</param>
        private async Task HandleXTrade(IRedisService redis, XTradeResponse? response)
        {
            try
            {
                string redisKey = $"{RedisConstants.REDIS_KEY_PREFIX_MARKET_DATA}:{response!.Symbol}";
                var existingData = await redis.GetHashAsync<MarketSymbolDto>(redisKey);

                if (existingData != null)
                {
                    await UpdateExistingTradeData(redis, redisKey, response, existingData);
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
        /// Applies incremental X-TRADE updates to an existing symbol snapshot.
        /// </summary>
        /// <param name="redis">Redis service abstraction.</param>
        /// <param name="redisKey">Redis key for current symbol market data.</param>
        /// <param name="response">Incoming X-TRADE payload.</param>
        /// <param name="existingData">Current cached market snapshot for symbol.</param>
        private async Task UpdateExistingTradeData(IRedisService redis, string redisKey, XTradeResponse response, MarketSymbolDto existingData)
        {
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

            // Calculate volume delta from cumulative TotalVol to identify real new trades.
            // Using LastVol alone can miss volume when SSI batches multiple executions.
            var newTotalVol = (long)(response.TotalVol ?? 0);

            // Math.Max(0, ...) protects from replay/out-of-order/session reset scenarios.
            long volDelta = Math.Max(0L, newTotalVol - (long)existingData.TotalVol);
            long newBuyVol = (long)existingData.TotalBuyVol;
            long newSellVol = (long)existingData.TotalSellVol;

            var sideUpper = (response.Side ?? string.Empty).ToUpperInvariant();
            if (volDelta > 0)
            {
                if (sideUpper.StartsWith("B"))
                {
                    newBuyVol += volDelta;
                }
                else if (sideUpper.StartsWith("S") || sideUpper.StartsWith("M"))
                {
                    newSellVol += volDelta;
                }
                else
                {
                    _logger.LogWarning(
                        "Unrecognized Side='{Side}' for {Ticker} volDelta={Vol} - not counted in Buy/Sell",
                        response.Side, response.Symbol, volDelta);
                }
            }

            updates[nameof(MarketSymbolDto.TotalBuyVol)] = newBuyVol;
            updates[nameof(MarketSymbolDto.TotalSellVol)] = newSellVol;

            if (updates.Count > 0)
            {
                QueueRedisHashFieldsWrite(redisKey, updates);

                var marketPayload = new Dictionary<string, object>(updates)
                {
                    ["Ticker"] = response.Symbol!,
                };
                QueueMarketBroadcast(response.Symbol!, marketPayload);

                var mergedData = CloneMarketData(existingData);
                ApplyFieldUpdatesToObject(mergedData, updates);
                mergedData.Ticker = response.Symbol!;
                QueuePriceDepthBroadcast(BuildPriceDepthDto(response.Symbol!, mergedData));
            }

            // Only persist/broadcast trade tape when cumulative volume actually increases.
            var isNewTrade = response.LastPrice.HasValue
                             && response.LastVol.HasValue
                             && response.LastPrice > 0
                             && volDelta > 0;

            if (isNewTrade)
            {
                // Parse SSI time "HHmmss" (UTC+7); fall back to current UTC+7 when missing.
                var rawTime = response.Time;
                string formattedTime;
                if (!string.IsNullOrWhiteSpace(rawTime) && rawTime.Length >= 6 && rawTime.All(char.IsDigit))
                {
                    formattedTime = $"{rawTime[0..2]}:{rawTime[2..4]}:{rawTime[4..6]}";
                }
                else
                {
                    formattedTime = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")).ToString("HH:mm:ss");
                }

                var trade = new RecentTradeDto
                {
                    Ticker = response.Symbol!,
                    Price = response.LastPrice!.Value,
                    Volume = response.LastVol!.Value,
                    Side = response.Side ?? string.Empty,
                    Time = formattedTime,
                };

                QueueRedisListPushTrimWrite(RedisConstants.Trades(response.Symbol!), trade, 200);
                QueueTradeBroadcast(trade);
            }
        }

        /// <summary>
        /// Initializes symbol trade snapshot when key does not exist yet in Redis.
        /// </summary>
        /// <param name="redis">Redis service abstraction.</param>
        /// <param name="redisKey">Redis key for current symbol market data.</param>
        /// <param name="response">Incoming X-TRADE payload.</param>
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
                TotalBuyVol = (response.Side ?? string.Empty).ToUpperInvariant().StartsWith("B") ? (response.LastVol ?? 0) : 0,
                TotalSellVol = ((response.Side ?? string.Empty).ToUpperInvariant().StartsWith("S")
                               || (response.Side ?? string.Empty).ToUpperInvariant().StartsWith("M")) ? (response.LastVol ?? 0) : 0,
            };

            QueueRedisHashObjectWrite(redisKey, newData);

            // Intentionally do not push to TRADES list here.
            // On reconnect, SSI replays last known X-TRADE payload. Recording it as a new trade
            // would create duplicates after restart/reconnect.
            await Task.CompletedTask;
        }
    }
}
