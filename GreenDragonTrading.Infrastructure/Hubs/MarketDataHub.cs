using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time market data streaming.
    /// This Hub handles client connections and group subscriptions.
    /// Business logic should be in Application layer, not here.
    /// </summary>
    public class MarketDataHub(
        ILogger<MarketDataHub> logger,
        IRedisService redisService) : Hub
    {
        private readonly ILogger<MarketDataHub> _logger = logger;
        private readonly IRedisService _redisService = redisService;

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId} from {IPAddress}", 
                Context.ConnectionId, 
                Context.GetHttpContext()?.Connection.RemoteIpAddress);
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                _logger.LogWarning(exception, "Client disconnected with error: {ConnectionId}", Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Subscribe to market data for specific symbols.
        /// Client will receive updates only for subscribed symbols.
        /// Automatically sends current market data for subscribed symbols.
        /// </summary>
        /// <param name="symbols">List of stock symbols to subscribe (e.g., ["VNM", "HPG", "VCB"])</param>
        public async Task SubscribeToSymbols(string[] symbols)
        {
            if (symbols == null || symbols.Length == 0)
            {
                _logger.LogWarning("Client {ConnectionId} attempted to subscribe with empty symbols", Context.ConnectionId);
                return;
            }

            var subscribedSymbols = new List<string>();

            foreach (var symbol in symbols)
            {
                if (string.IsNullOrWhiteSpace(symbol))
                    continue;

                var upperSymbol = symbol.ToUpper();
                await Groups.AddToGroupAsync(Context.ConnectionId, upperSymbol);
                subscribedSymbols.Add(upperSymbol);

                // Send initial data for this symbol
                try
                {
                    string redisKey = $"{RedisConstants.REDIS_KEY_PREFIX_MARKET_DATA}:{upperSymbol}";
                    var marketData = await _redisService.GetHashAsync<MarketSymbolDto>(redisKey);

                    if (marketData != null)
                    {
                        await Clients.Caller.SendAsync("ReceiveMarketData", marketData);
                        _logger.LogDebug("Sent initial data for {Symbol} to client {ConnectionId}", upperSymbol, Context.ConnectionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send initial data for {Symbol} to client {ConnectionId}", upperSymbol, Context.ConnectionId);
                }
            }

            _logger.LogInformation("Client {ConnectionId} subscribed to {Count} symbols: {Symbols}", 
                Context.ConnectionId, 
                subscribedSymbols.Count, 
                string.Join(", ", subscribedSymbols));
        }

        /// <summary>
        /// Unsubscribe from market data for specific symbols.
        /// </summary>
        /// <param name="symbols">List of stock symbols to unsubscribe</param>
        public async Task UnsubscribeFromSymbols(string[] symbols)
        {
            if (symbols == null || symbols.Length == 0)
            {
                return;
            }

            foreach (var symbol in from symbol in symbols
                                   where !string.IsNullOrWhiteSpace(symbol)
                                   select symbol)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, symbol.ToUpper());
            }

            _logger.LogInformation("Client {ConnectionId} unsubscribed from {Count} symbols: {Symbols}", 
                Context.ConnectionId, 
                symbols.Length, 
                string.Join(", ", symbols));
        }

        /// <summary>
        /// Subscribe to all market data (broadcast).
        /// Use with caution as this will receive all market updates.
        /// </summary>
        public async Task SubscribeToAll()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "ALL_MARKET_DATA");
            _logger.LogInformation("Client {ConnectionId} subscribed to all market data", Context.ConnectionId);
        }

        /// <summary>
        /// Unsubscribe from all market data.
        /// </summary>
        public async Task UnsubscribeFromAll()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ALL_MARKET_DATA");
            _logger.LogInformation("Client {ConnectionId} unsubscribed from all market data", Context.ConnectionId);
        }
    }
}
