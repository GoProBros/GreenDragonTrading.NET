using GreenDragonTrading.Infrastructure.Hubs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.DTOs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <inheritdoc/>
    public class MarketDataBroadcaster(
        IHubContext<MarketDataHub> hubContext,
        ILogger<MarketDataBroadcaster> logger) : IMarketDataBroadcaster
    {
        private readonly IHubContext<MarketDataHub> _hubContext = hubContext;
        private readonly ILogger<MarketDataBroadcaster> _logger = logger;

        /// <inheritdoc/>
        public async Task BroadcastMarketDataAsync<T>(string symbol, T data, CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients
                    .Group(symbol)
                    .SendAsync("ReceiveMarketData", data, cancellationToken);

                _logger.LogDebug("Broadcasted market data for symbol: {Symbol}", symbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting market data for symbol: {Symbol}", symbol);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task BroadcastOhlcvUpdateAsync(CurrentCandleDto candle, CancellationToken cancellationToken = default)
        {
            try
            {
                var groupName = $"OHLCV:{candle.Ticker}:{candle.Timeframe}";
                
                await _hubContext.Clients
                    .Group(groupName)
                    .SendAsync("ReceiveOhlcvUpdate", candle, cancellationToken);

                _logger.LogInformation(
                    "📊 Broadcasted {Status} {Timeframe} candle for {Ticker} - O:{Open} H:{High} L:{Low} C:{Close} V:{Volume}",
                    candle.IsComplete ? "completed" : "in-progress",
                    candle.Timeframe,
                    candle.Ticker,
                    candle.Open,
                    candle.High,
                    candle.Low,
                    candle.Close,
                    candle.Volume);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "❌ CRITICAL ERROR broadcasting OHLCV update for {Ticker} {Timeframe} to group {GroupName}. Exception: {Message}",
                    candle.Ticker, candle.Timeframe, $"OHLCV:{candle.Ticker}:{candle.Timeframe}", ex.Message);
                throw;
            }
        }
    }
}
