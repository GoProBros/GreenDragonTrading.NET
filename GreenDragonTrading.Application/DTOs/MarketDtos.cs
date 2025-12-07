using System.Text.Json.Serialization;

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

        public double? LastPrice { get; set; }

        public double? LastVol { get; set; }

        public double? Change { get; set; }

        public double? RatioChange{ get; set; }

        public double BidPrice1 { get; set; }

        public double BidVol1 { get; set; }

        public double BidPrice2 { get; set; }

        public double BidVol2 { get; set; }

        public double BidPrice3 { get; set; }

        public double BidVol3 { get; set; }

        public double? TotalVal { get; set; }

        public double? TotalVol { get; set; }

        public double? Highest { get; set; }

        public double? Lowest { get; set; }

        public string? Side { get; set; }




        public string? TradingSession { get; set; }

        public string? TradingStatus { get; set; }
    }
}
