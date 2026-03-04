namespace GreenDragonTrading.Application.DTOs
{
    public class MarketSymbolDto
    {
        public string Ticker { get; set; } = string.Empty;

        public double CeilingPrice { get; set; }

        public double FloorPrice { get; set; }

        public double ReferencePrice { get; set; }

        public double AskPrice1 { get; set; }

        public double AskVol1 { get; set; }

        public double AskPrice2 { get; set; }

        public double AskVol2 { get; set; }

        public double AskPrice3 { get; set; }

        public double AskVol3 { get; set; }

        public double LastPrice { get; set; }

        public double LastVol { get; set; }

        public double Change { get; set; }

        public double RatioChange{ get; set; }

        public double BidPrice1 { get; set; }

        public double BidVol1 { get; set; }

        public double BidPrice2 { get; set; }

        public double BidVol2 { get; set; }

        public double BidPrice3 { get; set; }

        public double BidVol3 { get; set; }

        public double TotalVal { get; set; }

        public double TotalVol { get; set; }

        public double Highest { get; set; }

        public double Lowest { get; set; }

        public string Side { get; set; } = string.Empty;

        public double AvgPrice { get; set; }

        public double PriorVal { get; set; }

        public double TotalRoom { get; set; }

        public double CurrentRoom { get; set; }

        public double FBuyVol { get; set; }

        public double FSellVol { get; set; }

        public double FBuyVal { get; set; }

        public double FSellVal { get; set; }

        public double TotalBuyVol { get; set; }

        public double TotalSellVol { get; set; }

        public string? TradingSession { get; set; }

        public string? TradingStatus { get; set; }
    }

    /// <summary>
    /// A single matched order record stored in Redis (TRADES:{ticker} list)
    /// </summary>
    public class RecentTradeDto
    {
        public string Ticker { get; set; } = string.Empty;
        public double Price { get; set; }
        public double Volume { get; set; }
        /// <summary>B = Sell (Bán), M = Buy (Mua), N = Neutral</summary>
        public string Side { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }

    public class IntradayOhlc
    {
        public string Symbol { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string TradingDate { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string Open { get; set; } = string.Empty;
        public string High { get; set; } = string.Empty;
        public string Low { get; set; } = string.Empty;
        public string Close { get; set; } = string.Empty;
        public string Volume { get; set; } = string.Empty;
    }

    /// <summary>
    /// Price depth snapshot for the "3 Bước Giá" module.
    /// Broadcast via SignalR group DEPTH:{ticker} event ReceivePriceDepth.
    /// </summary>
    public class PriceDepthDto
    {
        public string Ticker { get; set; } = string.Empty;

        // Ask (Sell) levels – ask1 = best (lowest price), ask3 = worst (highest)
        public double AskPrice1 { get; set; }
        public double AskVol1 { get; set; }
        public double AskPrice2 { get; set; }
        public double AskVol2 { get; set; }
        public double AskPrice3 { get; set; }
        public double AskVol3 { get; set; }

        // Bid (Buy) levels – bid1 = best (highest price), bid3 = worst (lowest)
        public double BidPrice1 { get; set; }
        public double BidVol1 { get; set; }
        public double BidPrice2 { get; set; }
        public double BidVol2 { get; set; }
        public double BidPrice3 { get; set; }
        public double BidVol3 { get; set; }

        // Reference / limit prices
        public double ReferencePrice { get; set; }
        public double CeilingPrice { get; set; }
        public double FloorPrice { get; set; }

        // Session-level totals
        public double Change { get; set; }
        public double RatioChange { get; set; }
        public double TotalVol { get; set; }

        // Per-level change vs reference price (raw, in same unit as price)
        public double AskChange1 { get; set; }
        public double AskChangePct1 { get; set; }
        public double AskChange2 { get; set; }
        public double AskChangePct2 { get; set; }
        public double AskChange3 { get; set; }
        public double AskChangePct3 { get; set; }
        public double BidChange1 { get; set; }
        public double BidChangePct1 { get; set; }
        public double BidChange2 { get; set; }
        public double BidChangePct2 { get; set; }
        public double BidChange3 { get; set; }
        public double BidChangePct3 { get; set; }

        // Foreign investor volume / value
        public double FBuyVol { get; set; }
        public double FSellVol { get; set; }
        public double FBuyVal { get; set; }
        public double FSellVal { get; set; }

        // Tổng KL mua / bán tích lũy từ X-Trade
        public double TotalBuyVol { get; set; }
        public double TotalSellVol { get; set; }

        // Pre-computed derived values (calculated server-side)
        /// <summary>Bull (bid) percentage of total depth volume (0-100)</summary>
        public int BullPct { get; set; }
        /// <summary>Bear (ask) percentage of total depth volume (0-100)</summary>
        public int BearPct { get; set; }
        /// <summary>Foreign buy percentage relative to total foreign volume (0-100)</summary>
        public double FBuyPct { get; set; }
        /// <summary>Foreign sell percentage relative to total foreign volume (0-100)</summary>
        public double FSellPct { get; set; }
        /// <summary>Maximum single-level depth volume (used by client to compute bar widths)</summary>
        public double MaxDepthVol { get; set; }

        public string Side { get; set; } = string.Empty;
        public string? TradingSession { get; set; }
    }
}
