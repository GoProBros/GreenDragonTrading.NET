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

        /// <summary>
        /// Subscribe to OHLCV updates for a specific ticker and timeframe.
        /// Client will receive real-time candle updates as they form.
        /// </summary>
        /// <param name="ticker">Stock ticker symbol (e.g., "FPT")</param>
        /// <param name="timeframe">Timeframe (M1, M5, M15, H1, H4, D1)</param>
        public async Task SubscribeToOhlcv(string ticker, string timeframe)
        {
            if (string.IsNullOrWhiteSpace(ticker) || string.IsNullOrWhiteSpace(timeframe))
            {
                _logger.LogWarning(
                    "Client {ConnectionId} attempted to subscribe with invalid ticker or timeframe",
                    Context.ConnectionId);
                return;
            }

            var upperTicker = ticker.ToUpper();
            var upperTimeframe = timeframe.ToUpper();
            var groupName = $"OHLCV:{upperTicker}:{upperTimeframe}";

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation(
                "Client {ConnectionId} subscribed to OHLCV {Ticker} {Timeframe}",
                Context.ConnectionId, upperTicker, upperTimeframe);

            // Send current candle immediately from Redis if exists
            try
            {
                string redisKey = $"OHLCV:{upperTicker}:{upperTimeframe}";
                var currentCandle = await _redisService.GetAsync<CurrentCandleDto>(redisKey);
                
                if (currentCandle != null && currentCandle.Volume > 0)
                {
                    await Clients.Caller.SendAsync("ReceiveCurrentCandle", currentCandle);
                    _logger.LogDebug(
                        "Sent current {Timeframe} candle for {Ticker} to client {ConnectionId} (Volume: {Volume})",
                        upperTimeframe, upperTicker, Context.ConnectionId, currentCandle.Volume);
                }
                else if (currentCandle != null && currentCandle.Volume == 0)
                {
                    _logger.LogDebug(
                        "Skipped sending empty {Timeframe} candle for {Ticker} to client {ConnectionId} (no data yet)",
                        upperTimeframe, upperTicker, Context.ConnectionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to send current candle for {Ticker} {Timeframe} to client {ConnectionId}",
                    upperTicker, upperTimeframe, Context.ConnectionId);
            }
        }

        /// <summary>
        /// Unsubscribe from OHLCV updates for a specific ticker and timeframe.
        /// </summary>
        /// <param name="ticker">Stock ticker symbol</param>
        /// <param name="timeframe">Timeframe</param>
        public async Task UnsubscribeFromOhlcv(string ticker, string timeframe)
        {
            if (string.IsNullOrWhiteSpace(ticker) || string.IsNullOrWhiteSpace(timeframe))
            {
                return;
            }

            var upperTicker = ticker.ToUpper();
            var upperTimeframe = timeframe.ToUpper();
            var groupName = $"OHLCV:{upperTicker}:{upperTimeframe}";

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation(
                "Client {ConnectionId} unsubscribed from OHLCV {Ticker} {Timeframe}",
                Context.ConnectionId, upperTicker, upperTimeframe);
        }

        /// <summary>
        /// Get all current candles for a ticker across all timeframes.
        /// Useful for displaying multiple timeframe charts simultaneously.
        /// </summary>
        /// <param name="ticker">Stock ticker symbol</param>
        public async Task GetAllCurrentCandles(string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker))
            {
                return;
            }

            try
            {
                var upperTicker = ticker.ToUpper();
                var timeframes = new[] { "M1", "M5", "M15", "M30", "H1", "H4", "D1", "W1", "MN1" };
                var candles = new List<CurrentCandleDto>();

                // Read all timeframes from Redis
                foreach (var tf in timeframes)
                {
                    var redisKey = $"OHLCV:{upperTicker}:{tf}";
                    var candle = await _redisService.GetAsync<CurrentCandleDto>(redisKey);
                    if (candle != null && candle.Volume > 0)
                    {
                        candles.Add(candle);
                    }
                }

                await Clients.Caller.SendAsync("ReceiveAllCurrentCandles", new
                {
                    Ticker = upperTicker,
                    Candles = candles
                });

                _logger.LogDebug(
                    "Sent {Count} current candles for {Ticker} to client {ConnectionId}",
                    candles.Count, upperTicker, Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to send all current candles for {Ticker} to client {ConnectionId}",
                    ticker, Context.ConnectionId);
            }
        }

        /// <summary>
        /// Subscribe to heatmap updates for a specific exchange and/or sector.
        /// Client will receive real-time per-symbol heatmap item updates.
        /// </summary>
        /// <param name="exchange">Exchange code (hsx, hnx, upcom) or null for all exchanges</param>
        /// <param name="sector">Sector ID or null for all sectors</param>
        public async Task SubscribeToHeatmap(string? exchange, string? sector)
        {
            var exchangeLower = exchange?.ToLower() ?? "all";
            
            // Subscribe to exchange-specific group (e.g., HEATMAP:hsx)
            var exchangeGroup = $"HEATMAP:{exchangeLower}";
            await Groups.AddToGroupAsync(Context.ConnectionId, exchangeGroup);
            
            // Also subscribe to sector-specific group if provided
            if (!string.IsNullOrEmpty(sector))
            {
                var sectorGroup = $"HEATMAP:{exchangeLower}:{sector}";
                await Groups.AddToGroupAsync(Context.ConnectionId, sectorGroup);
                
                _logger.LogInformation(
                    "Client {ConnectionId} subscribed to heatmap: Exchange={Exchange}, Sector={Sector}",
                    Context.ConnectionId, exchangeLower, sector);
            }
            else
            {
                _logger.LogInformation(
                    "Client {ConnectionId} subscribed to heatmap: Exchange={Exchange}",
                    Context.ConnectionId, exchangeLower);
            }
        }

        /// <summary>
        /// Unsubscribe from heatmap updates.
        /// </summary>
        /// <param name="exchange">Exchange code or null</param>
        /// <param name="sector">Sector ID or null</param>
        public async Task UnsubscribeFromHeatmap(string? exchange, string? sector)
        {
            var exchangeLower = exchange?.ToLower() ?? "all";
            
            // Unsubscribe from exchange-specific group
            var exchangeGroup = $"HEATMAP:{exchangeLower}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, exchangeGroup);
            
            // Also unsubscribe from sector-specific group if provided
            if (!string.IsNullOrEmpty(sector))
            {
                var sectorGroup = $"HEATMAP:{exchangeLower}:{sector}";
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, sectorGroup);
            }

            _logger.LogInformation(
                "Client {ConnectionId} unsubscribed from heatmap: Exchange={Exchange}, Sector={Sector}",
                Context.ConnectionId, exchangeLower, sector ?? "all");
        }

        /// <summary>
        /// Get current heatmap snapshot for specific exchange and/or sector.
        /// Returns current heatmap data from Redis without subscribing to updates.
        /// </summary>
        /// <param name="exchange">Exchange code or null for all</param>
        /// <param name="sector">Sector ID or null for all</param>
        public async Task<HeatmapDataDto> GetCurrentHeatmap(string? exchange, string? sector)
        {
            try
            {
                _logger.LogInformation(
                    "Client {ConnectionId} requested current heatmap: Exchange={Exchange}, Sector={Sector}",
                    Context.ConnectionId, exchange ?? "ALL", sector ?? "ALL");

                // Get all heatmap keys from Redis
                var pattern = "HEATMAP:*";
                var keys = await _redisService.GetKeysAsync(pattern);
                
                var heatmapItems = new List<HeatmapItemDto>();

                // Fetch all heatmap items from Redis
                foreach (var key in keys)
                {
                    try
                    {
                        var item = await _redisService.GetAsync<HeatmapItemDto>(key);
                        if (item != null)
                        {
                            // Apply filters
                            bool matchExchange = string.IsNullOrEmpty(exchange) || 
                                                item.Exchange.Equals(exchange, StringComparison.OrdinalIgnoreCase);
                            bool matchSector = string.IsNullOrEmpty(sector) || 
                                              (item.Sector != null && item.Sector.Equals(sector, StringComparison.OrdinalIgnoreCase));

                            if (matchExchange && matchSector)
                            {
                                heatmapItems.Add(item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to fetch heatmap item from key {Key}", key);
                    }
                }

                var result = new HeatmapDataDto
                {
                    Exchange = exchange,
                    Sector = sector,
                    Items = heatmapItems,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation(
                    "Returned {Count} heatmap items to client {ConnectionId}",
                    heatmapItems.Count, Context.ConnectionId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error getting current heatmap for client {ConnectionId}",
                    Context.ConnectionId);
                
                // Return empty result on error
                return new HeatmapDataDto
                {
                    Exchange = exchange,
                    Sector = sector,
                    Items = new List<HeatmapItemDto>(),
                    Timestamp = DateTime.UtcNow
                };
            }
        }
    }
}
