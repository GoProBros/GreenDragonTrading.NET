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
}
