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
                    string redisKey = RedisConstants.MarketDataSymbol(upperSymbol);
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
            var groupName = RedisConstants.OhlcvSignalRGroup(upperTicker, upperTimeframe);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation(
                "Client {ConnectionId} subscribed to OHLCV {Ticker} {Timeframe}",
                Context.ConnectionId, upperTicker, upperTimeframe);

            // Send current candle immediately from Redis if exists
            try
            {
                string redisKey = RedisConstants.Ohlcv(upperTicker, upperTimeframe);
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
            var groupName = RedisConstants.OhlcvSignalRGroup(upperTicker, upperTimeframe);

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
                    var redisKey = RedisConstants.Ohlcv(upperTicker, tf);
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
            var exchangeGroup = RedisConstants.HeatmapExchangeGroup(exchangeLower);
            await Groups.AddToGroupAsync(Context.ConnectionId, exchangeGroup);
            
            // Also subscribe to sector-specific group if provided
            if (!string.IsNullOrEmpty(sector))
            {
                var sectorGroup = RedisConstants.HeatmapSectorGroup(exchangeLower, sector);
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
            var exchangeGroup = RedisConstants.HeatmapExchangeGroup(exchangeLower);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, exchangeGroup);
            
            // Also unsubscribe from sector-specific group if provided
            if (!string.IsNullOrEmpty(sector))
            {
                var sectorGroup = RedisConstants.HeatmapSectorGroup(exchangeLower, sector);
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
                var pattern = RedisConstants.HeatmapPattern();
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

        /// <summary>
        /// Subscribe to real-time matched orders for a specific ticker.
        /// Immediately sends the last 20 trades from Redis, then pushes new trades via ReceiveTradeData.
        /// </summary>
        /// <param name="ticker">Stock ticker symbol (e.g. "FPT")</param>
        public async Task SubscribeToTradeUpdates(string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker)) return;

            var upper = ticker.ToUpper();
            var groupName = RedisConstants.TradeSignalRGroup(upper);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation("Client {ConnectionId} subscribed to trade updates for {Ticker}",
                Context.ConnectionId, upper);

            // Send initial trade history from Redis
            try
            {
                var key = RedisConstants.Trades(upper);
                var trades = await _redisService.ListRangeAsync<RecentTradeDto>(key, 200);
                if (trades.Count > 0)
                {
                    await Clients.Caller.SendAsync("ReceiveRecentTrades", trades);
                    _logger.LogDebug("Sent {Count} recent trades for {Ticker} to {ConnectionId}",
                        trades.Count, upper, Context.ConnectionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send initial trades for {Ticker}", upper);
            }
        }

        /// <summary>
        /// Unsubscribe from real-time matched orders for a specific ticker.
        /// </summary>
        /// <param name="ticker">Stock ticker symbol</param>
        public async Task UnsubscribeFromTradeUpdates(string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker)) return;

            var groupName = RedisConstants.TradeSignalRGroup(ticker);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation("Client {ConnectionId} unsubscribed from trade updates for {Ticker}",
                Context.ConnectionId, ticker.ToUpper());
        }

        /// <summary>
        /// Subscribe to price depth (3 bước giá) updates for a specific ticker.
        /// Server sends a snapshot of the current price depth immediately via ReceivePriceDepth,
        /// then pushes on every bid/ask or snapshot change.
        /// </summary>
        /// <param name="ticker">Stock ticker symbol (e.g. "FPT")</param>
        public async Task SubscribeToPriceDepth(string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker)) return;

            var upper = ticker.ToUpper();
            var groupName = RedisConstants.DepthSignalRGroup(upper);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation("Client {ConnectionId} subscribed to price depth for {Ticker}",
                Context.ConnectionId, upper);

            // Send current price depth snapshot from Redis
            try
            {
                string redisKey = $"{RedisConstants.REDIS_KEY_PREFIX_MARKET_DATA}:{upper}";
                var marketData = await _redisService.GetHashAsync<MarketSymbolDto>(redisKey);

                if (marketData != null)
                {
                    var depth = BuildPriceDepthDto(upper, marketData);
                    await Clients.Caller.SendAsync("ReceivePriceDepth", depth);
                    _logger.LogDebug("Sent initial price depth for {Ticker} to {ConnectionId}", upper, Context.ConnectionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send initial price depth for {Ticker}", upper);
            }
        }

        /// <summary>
        /// Unsubscribe from price depth updates for a specific ticker.
        /// </summary>
        /// <param name="ticker">Stock ticker symbol</param>
        public async Task UnsubscribeFromPriceDepth(string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker)) return;

            var groupName = RedisConstants.DepthSignalRGroup(ticker.ToUpper());
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation("Client {ConnectionId} unsubscribed from price depth for {Ticker}",
                Context.ConnectionId, ticker.ToUpper());
        }

        /// <summary>
        /// Subscribe to real-time market index updates (VNINDEX, VN30, HNX30, …).
        /// Joins the client to INDEX:{CODE} groups and immediately sends cached snapshots for each code.
        /// </summary>
        /// <param name="codes">Array of index codes (e.g. ["VNINDEX", "VN30"]).</param>
        public async Task SubscribeToIndices(string[] codes)
        {
            if (codes == null || codes.Length == 0) return;

            foreach (var rawCode in codes)
            {
                if (string.IsNullOrWhiteSpace(rawCode)) continue;

                var upper = rawCode.ToUpperInvariant();
                var groupName = RedisConstants.IndexSignalRGroup(upper);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                // Send current cached snapshot immediately so the client does not have to wait
                try
                {
                    var redisKey = RedisConstants.IndexData(upper);
                    var snapshot = await _redisService.GetAsync<LiveIndexDataDto>(redisKey);
                    if (snapshot != null)
                    {
                        await Clients.Caller.SendAsync("ReceiveIndexData", snapshot);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send initial index snapshot for {Code}", upper);
                }
            }

            _logger.LogInformation("Client {ConnectionId} subscribed to indices: {Codes}",
                Context.ConnectionId,
                string.Join(", ", codes.Select(c => c.ToUpperInvariant())));
        }

        /// <summary>
        /// Unsubscribe from market index updates.
        /// </summary>
        /// <param name="codes">Array of index codes to unsubscribe from.</param>
        public async Task UnsubscribeFromIndices(string[] codes)
        {
            if (codes == null || codes.Length == 0) return;

            foreach (var rawCode in codes)
            {
                if (string.IsNullOrWhiteSpace(rawCode)) continue;

                var upper = rawCode.ToUpperInvariant();
                var groupName = RedisConstants.IndexSignalRGroup(upper);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            }

            _logger.LogInformation("Client {ConnectionId} unsubscribed from indices: {Codes}",
                Context.ConnectionId,
                string.Join(", ", codes.Select(c => c.ToUpperInvariant())));
        }

        /// <summary>
        /// Builds a PriceDepthDto from a MarketSymbolDto by calculating per-level changes.
        /// </summary>
        private static PriceDepthDto BuildPriceDepthDto(string ticker, MarketSymbolDto d)
        {
            var refPrice = d.ReferencePrice;

            double Chg(double price) => price > 0 && refPrice > 0 ? price - refPrice : 0;
            double ChgPct(double price) => price > 0 && refPrice > 0 ? ((price - refPrice) / refPrice) * 100 : 0;

            // Bull / Bear depth ratio
            var bidSum = d.BidVol1 + d.BidVol2 + d.BidVol3;
            var askSum = d.AskVol1 + d.AskVol2 + d.AskVol3;
            var depthTotal = bidSum + askSum;
            var bullPct = depthTotal > 0 ? (int)Math.Round((bidSum / depthTotal) * 100) : 50;

            // Foreign investor ratio vs total session volume
            var fBuyPct  = d.TotalVol > 0 ? Math.Round((d.FBuyVol  / d.TotalVol) * 100, 1) : 0.0;
            var fSellPct = d.TotalVol > 0 ? Math.Round((d.FSellVol / d.TotalVol) * 100, 1) : 0.0;

            // Max single-level depth volume (for bar width scaling)
            var maxDepthVol = Math.Max(1, new[] { d.AskVol1, d.AskVol2, d.AskVol3, d.BidVol1, d.BidVol2, d.BidVol3 }.Max());

            return new PriceDepthDto
            {
                Ticker = ticker,
                AskPrice1 = d.AskPrice1, AskVol1 = d.AskVol1,
                AskPrice2 = d.AskPrice2, AskVol2 = d.AskVol2,
                AskPrice3 = d.AskPrice3, AskVol3 = d.AskVol3,
                BidPrice1 = d.BidPrice1, BidVol1 = d.BidVol1,
                BidPrice2 = d.BidPrice2, BidVol2 = d.BidVol2,
                BidPrice3 = d.BidPrice3, BidVol3 = d.BidVol3,
                ReferencePrice = refPrice,
                CeilingPrice = d.CeilingPrice,
                FloorPrice = d.FloorPrice,
                Change = d.Change,
                RatioChange = d.RatioChange,
                TotalVol = d.TotalVol,
                AskChange1 = Chg(d.AskPrice1), AskChangePct1 = ChgPct(d.AskPrice1),
                AskChange2 = Chg(d.AskPrice2), AskChangePct2 = ChgPct(d.AskPrice2),
                AskChange3 = Chg(d.AskPrice3), AskChangePct3 = ChgPct(d.AskPrice3),
                BidChange1 = Chg(d.BidPrice1), BidChangePct1 = ChgPct(d.BidPrice1),
                BidChange2 = Chg(d.BidPrice2), BidChangePct2 = ChgPct(d.BidPrice2),
                BidChange3 = Chg(d.BidPrice3), BidChangePct3 = ChgPct(d.BidPrice3),
                FBuyVol = d.FBuyVol, FSellVol = d.FSellVol,
                FBuyVal = d.FBuyVal, FSellVal = d.FSellVal,
                TotalBuyVol = d.TotalBuyVol,
                TotalSellVol = d.TotalSellVol,
                BullPct = bullPct, BearPct = 100 - bullPct,
                FBuyPct = fBuyPct, FSellPct = fSellPct,
                MaxDepthVol = maxDepthVol,
                Side = d.Side,
                TradingSession = d.TradingSession,
            };
        }
    }
}
