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

                // _logger.LogInformation(
                //     "📊 Broadcasted {Status} {Timeframe} candle for {Ticker} - O:{Open} H:{High} L:{Low} C:{Close} V:{Volume}",
                //     candle.IsComplete ? "completed" : "in-progress",
                //     candle.Timeframe,
                //     candle.Ticker,
                //     candle.Open,
                //     candle.High,
                //     candle.Low,
                //     candle.Close,
                //     candle.Volume);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "❌ CRITICAL ERROR broadcasting OHLCV update for {Ticker} {Timeframe} to group {GroupName}. Exception: {Message}",
                    candle.Ticker, candle.Timeframe, $"OHLCV:{candle.Ticker}:{candle.Timeframe}", ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task BroadcastHeatmapUpdateAsync(HeatmapDataDto heatmapData, string? exchange = null, string? sector = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var groupName = GetHeatmapGroupName(exchange, sector);
                
                await _hubContext.Clients
                    .Group(groupName)
                    .SendAsync("ReceiveHeatmapData", heatmapData, cancellationToken);

                _logger.LogDebug(
                    "📊 Broadcasted heatmap update to {GroupName}: {Count} items",
                    groupName, heatmapData.TotalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "❌ Error broadcasting heatmap update to {GroupName}",
                    GetHeatmapGroupName(exchange, sector));
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task BroadcastHeatmapItemAsync(HeatmapItemDto item, CancellationToken cancellationToken = default)
        {
            try
            {
                // Broadcast to exchange-specific group (e.g., HEATMAP:hsx)
                var exchangeGroup = $"HEATMAP:{item.Exchange.ToLower()}";
                await _hubContext.Clients
                    .Group(exchangeGroup)
                    .SendAsync("ReceiveHeatmapItem", item, cancellationToken);

                // Also broadcast to sector-specific group if applicable
                if (!string.IsNullOrEmpty(item.Sector))
                {
                    var sectorGroup = $"HEATMAP:{item.Exchange.ToLower()}:{item.Sector}";
                    await _hubContext.Clients
                        .Group(sectorGroup)
                        .SendAsync("ReceiveHeatmapItem", item, cancellationToken);
                }

                _logger.LogDebug(
                    "📊 Broadcasted heatmap item: {Ticker} ({Exchange}) - {ChangePercent}%",
                    item.Ticker, item.Exchange, item.ChangePercent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "❌ Error broadcasting heatmap item for {Ticker}",
                    item.Ticker);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task BroadcastTradeAsync(RecentTradeDto trade, CancellationToken cancellationToken = default)
        {
            try
            {
                var groupName = $"TRADE:{trade.Ticker.ToUpper()}";
                await _hubContext.Clients
                    .Group(groupName)
                    .SendAsync("ReceiveTradeData", trade, cancellationToken);

                _logger.LogDebug("Broadcasted trade for {Ticker}: {Side} {Price} x {Volume}",
                    trade.Ticker, trade.Side, trade.Price, trade.Volume);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting trade for {Ticker}", trade.Ticker);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task BroadcastPriceDepthAsync(PriceDepthDto depth, CancellationToken cancellationToken = default)
        {
            try
            {
                var groupName = $"DEPTH:{depth.Ticker.ToUpper()}";
                await _hubContext.Clients
                    .Group(groupName)
                    .SendAsync("ReceivePriceDepth", depth, cancellationToken);

                _logger.LogDebug("Broadcasted price depth for {Ticker}: Bid1={Bid1} Ask1={Ask1}",
                    depth.Ticker, depth.BidPrice1, depth.AskPrice1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting price depth for {Ticker}", depth.Ticker);
                throw;
            }
        }

        /// <summary>
        /// Get heatmap group name for SignalR groups
        /// </summary>
        private static string GetHeatmapGroupName(string? exchange, string? sector)
        {
            if (!string.IsNullOrEmpty(sector))
                return $"HEATMAP:{exchange}:{sector}";
            if (!string.IsNullOrEmpty(exchange))
                return $"HEATMAP:{exchange}";
            return "HEATMAP:ALL";
        }
    }
}
