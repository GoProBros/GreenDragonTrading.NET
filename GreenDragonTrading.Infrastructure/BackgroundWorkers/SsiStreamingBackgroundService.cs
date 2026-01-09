using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Constants.SSI;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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
                    //await HandleXTrade(_redis, response);
                }
                else if (string.Equals(wrapperResponse.DataType, SsiConstantsV2.SSI_STREAMING_DATA_TYPE_FOREIGN))
                {
                    var response = JsonSerializer.Deserialize<ForeignRoomResponse>(wrapperResponse.Content!);
                    //await HandleForeignRoom(_redis, response);
                }
                else if (string.Equals(wrapperResponse.DataType, SsiConstantsV2.SSI_STREAMING_DATA_TYPE_X))
                {
                    var response = JsonSerializer.Deserialize<SecuritiesSnapshot>(wrapperResponse.Content!);
                    await HandleSnapshot(_redis, response);
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
        private async Task HandleError(string error)
        {
            _logger.LogError("SSI Streaming error: {Error}", error);
        }

        /// <summary>
        /// Handle state changes in the streaming connection.
        /// </summary>
        /// <param name="oldState">Old streaming state</param>
        /// <param name="newState">New streaming state</param>
        /// <returns></returns>
        private async Task HandleStateChanged(string oldState, string newState)
        {
            _logger.LogInformation("SSI connection state changed: {OldState} -> {NewState}", oldState, newState);
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
                await _broadcaster.BroadcastMarketDataAsync(response.Symbol!, updates);
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
            await _broadcaster.BroadcastMarketDataAsync(response.Symbol!, newData);
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

            if (updates.Count > 0)
            {
                await redis.SetHashFieldsAsync(redisKey, updates);
            }

            updates["Ticker"] = response.Symbol!;
            await _broadcaster.BroadcastMarketDataAsync(response.Symbol!, updates);
        }

        /// <summary>
        /// If no existing trade data, create new entry in Redis.
        /// </summary>
        /// <param name="redis">Redis service</param>
        /// <param name="redisKey">Redis key to retrieve symbol data</param>
        /// <param name="response">X-TRADE data from SSI</param
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
                PriorVal = response.PriorVal ?? default
            };
            await redis.SetHashAsync(redisKey, newData);
            await _broadcaster.BroadcastMarketDataAsync(response.Symbol!, newData);
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
            await _broadcaster.BroadcastMarketDataAsync(response.Symbol!, updates);
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
            await _broadcaster.BroadcastMarketDataAsync(response.Symbol!, newData);
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
            AddIfChanged(updates, nameof(MarketSymbolDto.Side), response.Side, existingData.Side);

            if (updates.Count > 0)
            {
                await redis.SetHashFieldsAsync(redisKey, updates);
            }

            updates["Ticker"] = response.Symbol!;
            await _broadcaster.BroadcastMarketDataAsync(response.Symbol!, updates);
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
            await _broadcaster.BroadcastMarketDataAsync(response.Symbol!, newData);
        }

        #endregion Handle Snapshot

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
    }
}
