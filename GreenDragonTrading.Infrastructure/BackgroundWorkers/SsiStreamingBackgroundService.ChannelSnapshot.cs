using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    public partial class SsiStreamingBackgroundService
    {
        /// <summary>
        /// Handles X snapshot channel messages and merges orderbook/price/session fields.
        /// </summary>
        /// <param name="redis">Redis service abstraction.</param>
        /// <param name="response">Deserialized snapshot payload from SSI.</param>
        private async Task HandleSnapshot(IRedisService redis, SecuritiesSnapshot? response)
        {
            try
            {
                string redisKey = $"{RedisConstants.REDIS_KEY_PREFIX_MARKET_DATA}:{response!.Symbol}";
                var existingData = await redis.GetHashAsync<MarketSymbolDto>(redisKey);

                if (existingData != null)
                {
                    await UpdateExistingSnapshotData(redis, redisKey, response, existingData);
                }
                else
                {
                    await CreateNewSnapshotData(redis, redisKey, response);
                }
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling snapshot data for symbol: {Symbol}", response?.Symbol);
            }
        }

        /// <summary>
        /// Applies snapshot field changes to an existing market symbol record.
        /// </summary>
        /// <param name="redis">Redis service abstraction.</param>
        /// <param name="redisKey">Redis key for current symbol market data.</param>
        /// <param name="response">Incoming snapshot payload.</param>
        /// <param name="existingData">Current cached market snapshot for symbol.</param>
        private async Task UpdateExistingSnapshotData(IRedisService redis, string redisKey, SecuritiesSnapshot response, MarketSymbolDto existingData)
        {
            var updates = new Dictionary<string, object>();

            AddIfChanged(updates, nameof(MarketSymbolDto.CeilingPrice), response.Ceiling, existingData.CeilingPrice);
            AddIfChanged(updates, nameof(MarketSymbolDto.FloorPrice), response.Floor, existingData.FloorPrice);
            AddIfChanged(updates, nameof(MarketSymbolDto.ReferencePrice), response.RefPrice, existingData.ReferencePrice);
            AddIfChanged(updates, nameof(MarketSymbolDto.LastPrice), response.LastVal, existingData.LastPrice);
            AddIfChanged(updates, nameof(MarketSymbolDto.LastVol), response.LastVol, existingData.LastVol);
            AddIfChanged(updates, nameof(MarketSymbolDto.PriorVal), response.PriorVal, existingData.PriorVal);
            AddIfChanged(updates, nameof(MarketSymbolDto.Highest), response.High, existingData.Highest);
            AddIfChanged(updates, nameof(MarketSymbolDto.Lowest), response.Low, existingData.Lowest);
            AddIfChanged(updates, nameof(MarketSymbolDto.Change), response.Change, existingData.Change);
            AddIfChanged(updates, nameof(MarketSymbolDto.RatioChange), response.RatioChange, existingData.RatioChange);

            AddIfChanged(updates, nameof(MarketSymbolDto.TotalVal), response.TotalVal, existingData.TotalVal);
            AddIfChanged(updates, nameof(MarketSymbolDto.TotalVol), response.TotalVol, existingData.TotalVol);

            AddIfChanged(updates, nameof(MarketSymbolDto.BidPrice1), response.BidPrice1, existingData.BidPrice1);
            AddIfChanged(updates, nameof(MarketSymbolDto.BidVol1), response.BidVol1, existingData.BidVol1);
            AddIfChanged(updates, nameof(MarketSymbolDto.BidPrice2), response.BidPrice2, existingData.BidPrice2);
            AddIfChanged(updates, nameof(MarketSymbolDto.BidVol2), response.BidVol2, existingData.BidVol2);
            AddIfChanged(updates, nameof(MarketSymbolDto.BidPrice3), response.BidPrice3, existingData.BidPrice3);
            AddIfChanged(updates, nameof(MarketSymbolDto.BidVol3), response.BidVol3, existingData.BidVol3);

            AddIfChanged(updates, nameof(MarketSymbolDto.AskPrice1), response.AskPrice1, existingData.AskPrice1);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskVol1), response.AskVol1, existingData.AskVol1);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskPrice2), response.AskPrice2, existingData.AskPrice2);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskVol2), response.AskVol2, existingData.AskVol2);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskPrice3), response.AskPrice3, existingData.AskPrice3);
            AddIfChanged(updates, nameof(MarketSymbolDto.AskVol3), response.AskVol3, existingData.AskVol3);

            AddIfChanged(updates, nameof(MarketSymbolDto.TradingSession), response.TradingSession, existingData.TradingSession ?? string.Empty);
            AddIfChanged(updates, nameof(MarketSymbolDto.TradingStatus), response.TradingStatus, existingData.TradingStatus ?? string.Empty);

            if (updates.Count > 0)
            {
                QueueRedisHashFieldsWrite(redisKey, updates);

                // Keep heatmap key in sync with market snapshot updates.
                _ = await UpdateHeatmapRedisAsync(redis, response.Symbol!, updates, existingData);

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
        }

        /// <summary>
        /// Creates initial snapshot market data when symbol key does not exist in Redis.
        /// </summary>
        /// <param name="redis">Redis service abstraction.</param>
        /// <param name="redisKey">Redis key for current symbol market data.</param>
        /// <param name="response">Incoming snapshot payload.</param>
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
                Side = response.Side ?? string.Empty,
            };

            QueueRedisHashObjectWrite(redisKey, newData);
            await CreateInitialHeatmapRedisAsync(redis, response.Symbol!, newData);
            QueueMarketBroadcast(response.Symbol!, newData);
            QueuePriceDepthBroadcast(BuildPriceDepthDto(response.Symbol!, newData));
        }
    }
}
