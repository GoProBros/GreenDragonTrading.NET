using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Constants.SSI;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
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

        public SsiStreamingBackgroundService(
            ISsiStreamingService streamingService,
            ILogger<SsiStreamingBackgroundService> logger,
            IServiceScopeFactory serviceScopeFactory )
        {
            _streamingService = streamingService;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;

            _streamingService.OnBroadcastReceived += async (data) => await HandleBroadcast(data);
            _streamingService.OnErrorReceived += HandleError;
            _streamingService.OnStateChanged += HandleStateChanged;
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
        }

        private async Task HandleBroadcast(string data)
        {
            try
            {
                StreamingWrapperResponse wrapperResponse = JsonSerializer.Deserialize<StreamingWrapperResponse>(data)!;

                if (string.Equals(wrapperResponse.DataType, SsiConstantsV2.SSI_STREAMING_DATA_TYPE_X_QUOTE))
                {
                    var response = JsonSerializer.Deserialize<XQuoteResponse>(wrapperResponse.Content!);

                    using var scope = _serviceScopeFactory.CreateScope();
                    var _redis = scope.ServiceProvider.GetRequiredService<IRedisService>();
                    await HandleXQuote(_redis, response);
                }
                else if (string.Equals(wrapperResponse.DataType, SsiConstantsV2.SSI_STREAMING_DATA_TYPE_X_TRADE))
                {
                    var result = JsonSerializer.Deserialize<XTradeResponse>(wrapperResponse.Content!);
                }
                else if (string.Equals(wrapperResponse.DataType, SsiConstantsV2.SSI_STREAMING_DATA_TYPE_FOREIGN))
                {
                    var result = JsonSerializer.Deserialize<ForeignRoomResponse>(wrapperResponse.Content!);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing streaming data: {Data}", data);
            }
        }

        private async void HandleError(string error)
        {
            _logger.LogError("SSI Streaming error: {Error}", error);
            
            // Notify all connected clients about the error
        }

        private async void HandleStateChanged(string oldState, string newState)
        {
            _logger.LogInformation("SSI connection state changed: {OldState} -> {NewState}", oldState, newState);
            
            // Notify all connected clients about connection state change
        }

        public override void Dispose()
        {
            // Unsubscribe from events
            _streamingService.OnBroadcastReceived -= async (data) => await HandleBroadcast(data);
            _streamingService.OnErrorReceived -= HandleError;
            _streamingService.OnStateChanged -= HandleStateChanged;

            base.Dispose();
        }

        private async Task HandleXQuote(IRedisService redis, XQuoteResponse? response)
        {
            try
            {
                string redisKey = $"{RedisConstants.REDIS_KEY_PREFIX_MARKET_DATA}:{response!.Symbol}";

                if (await redis.ExistsAsync(redisKey))
                {
                    MarketSymbolDto existingData = (await redis.GetHashAsync<MarketSymbolDto>(redisKey))!;
                    existingData.BidPrice1 = response.BidPrice1 ?? default;
                    existingData.BidVol1 = response.BidVol1 ?? default;
                    existingData.AskPrice1 = response.AskPrice1 ?? default;
                    existingData.AskVol1 = response.AskVol1 ?? default;
                    existingData.BidPrice2 = response.BidPrice2 ?? default;
                    existingData.BidVol2 = response.BidVol2 ?? default;
                    existingData.AskPrice2 = response.AskPrice2 ?? default;
                    existingData.AskVol2 = response.AskVol2 ?? default;
                    existingData.BidPrice3 = response.BidPrice3 ?? default;
                    existingData.BidVol3 = response.BidVol3 ?? default;
                    existingData.AskPrice3 = response.AskPrice3 ?? default;
                    existingData.AskVol3 = response.AskVol3 ?? default;
                    await redis.SetHashAsync(redisKey, existingData);
                }
                else
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
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling X-QUOTE data for symbol: {Symbol}", response?.Symbol);
            }
        }
    }
}
