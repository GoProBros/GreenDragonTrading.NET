using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    public partial class SsiStreamingBackgroundService
    {
        /// <summary>
        /// Handles foreign-room channel updates and upserts corresponding market fields.
        /// </summary>
        /// <param name="redis">Redis service abstraction.</param>
        /// <param name="response">Deserialized foreign-room payload from SSI.</param>
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
                _logger.LogError(ex, "Error handling foreign room data for symbol: {Symbol}", response?.Symbol);
            }
        }

        /// <summary>
        /// Applies foreign-room field changes for an existing market symbol record.
        /// </summary>
        /// <param name="redis">Redis service abstraction.</param>
        /// <param name="redisKey">Redis key for current symbol market data.</param>
        /// <param name="response">Incoming foreign-room payload.</param>
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
                QueueRedisHashFieldsWrite(redisKey, updates);
            }

            var marketPayload = new Dictionary<string, object>(updates)
            {
                ["Ticker"] = response.Symbol!,
            };
            QueueMarketBroadcast(response.Symbol!, marketPayload);
        }

        /// <summary>
        /// Creates initial foreign-room market data when symbol key does not exist in Redis.
        /// </summary>
        /// <param name="redis">Redis service abstraction.</param>
        /// <param name="redisKey">Redis key for current symbol market data.</param>
        /// <param name="response">Incoming foreign-room payload.</param>
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

            QueueRedisHashObjectWrite(redisKey, newData);
            QueueMarketBroadcast(response.Symbol!, newData);
            await Task.CompletedTask;
        }
    }
}
