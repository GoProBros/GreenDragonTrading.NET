using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.DTOs
{
    public class StreamingWrapperResponse
    {
        [JsonPropertyName("DataType")]
        public string? DataType { get; set; }

        [JsonPropertyName("Content")]
        public string? Content { get; set; }
    }

    public class XQuoteResponse
    {
        [JsonPropertyName("TradingDate")]
        public string? TradingDate { get; set; }

        [JsonPropertyName("Time")]
        public string? Time { get; set; }

        [JsonPropertyName("Exchange")]
        public string? Exchange { get; set; }

        [JsonPropertyName("Symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("RType")]
        public string? RType { get; set; }

        //Ask prices
        [JsonPropertyName("AskPrice1")]
        public double? AskPrice1 { get; set; }

        [JsonPropertyName("AskPrice2")]
        public double? AskPrice2 { get; set; }

        [JsonPropertyName("AskPrice3")]
        public double? AskPrice3 { get; set; }

        [JsonPropertyName("AskPrice4")]
        public double? AskPrice4 { get; set; }

        [JsonPropertyName("AskPrice5")]
        public double? AskPrice5 { get; set; }

        [JsonPropertyName("AskPrice6")]
        public double? AskPrice6 { get; set; }

        [JsonPropertyName("AskPrice7")]
        public double? AskPrice7 { get; set; }

        [JsonPropertyName("AskPrice8")]
        public double? AskPrice8 { get; set; }

        [JsonPropertyName("AskPrice9")]
        public double? AskPrice9 { get; set; }

        [JsonPropertyName("AskPrice10")]
        public double? AskPrice10 { get; set; }

        //Ask volumes
        [JsonPropertyName("AskVol1")]
        public double? AskVol1 { get; set; }

        [JsonPropertyName("AskVol2")]
        public double? AskVol2 { get; set; }

        [JsonPropertyName("AskVol3")]
        public double? AskVol3 { get; set; }

        [JsonPropertyName("AskVol4")]
        public double? AskVol4 { get; set; }

        [JsonPropertyName("AskVol5")]
        public double? AskVol5 { get; set; }

        [JsonPropertyName("AskVol6")]
        public double? AskVol6 { get; set; }

        [JsonPropertyName("AskVol7")]
        public double? AskVol7 { get; set; }

        [JsonPropertyName("AskVol8")]
        public double? AskVol8 { get; set; }

        [JsonPropertyName("AskVol9")]
        public double? AskVol9 { get; set; }

        [JsonPropertyName("AskVol10")]
        public double? AskVol10 { get; set; }

        //Bid prices
        [JsonPropertyName("BidPrice1")]
        public double? BidPrice1 { get; set; }

        [JsonPropertyName("BidPrice2")]
        public double? BidPrice2 { get; set; }

        [JsonPropertyName("BidPrice3")]
        public double? BidPrice3 { get; set; }

        [JsonPropertyName("BidPrice4")]
        public double? BidPrice4 { get; set; }

        [JsonPropertyName("BidPrice5")]
        public double? BidPrice5 { get; set; }

        [JsonPropertyName("BidPrice6")]
        public double? BidPrice6 { get; set; }

        [JsonPropertyName("BidPrice7")]
        public double? BidPrice7 { get; set; }

        [JsonPropertyName("BidPrice8")]
        public double? BidPrice8 { get; set; }

        [JsonPropertyName("BidPrice9")]
        public double? BidPrice9 { get; set; }

        [JsonPropertyName("BidPrice10")]
        public double? BidPrice10 { get; set; }

        //Bid volumes
        [JsonPropertyName("BidVol1")]
        public double? BidVol1 { get; set; }

        [JsonPropertyName("BidVol2")]
        public double? BidVol2 { get; set; }

        [JsonPropertyName("BidVol3")]
        public double? BidVol3 { get; set; }

        [JsonPropertyName("BidVol4")]
        public double? BidVol4 { get; set; }

        [JsonPropertyName("BidVol5")]
        public double? BidVol5 { get; set; }

        [JsonPropertyName("BidVol6")]
        public double? BidVol6 { get; set; }

        [JsonPropertyName("BidVol7")]
        public double? BidVol7 { get; set; }

        [JsonPropertyName("BidVol8")]
        public double? BidVol8 { get; set; }

        [JsonPropertyName("BidVol9")]
        public double? BidVol9 { get; set; }

        [JsonPropertyName("BidVol10")]
        public double? BidVol10 { get; set; }

        [JsonPropertyName("TradingSession")]
        public string? TradingSession { get; set; }
    }

    public class XTradeResponse
    {
        [JsonPropertyName("RType")]
        public string? RType { get; set; }

        [JsonPropertyName("TradingDate")]
        public string? TradingDate { get; set; }

        [JsonPropertyName("Time")]
        public string? Time { get; set; }

        [JsonPropertyName("Isin")]
        public string? Isin { get; set; }

        [JsonPropertyName("Symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("Ceiling")]
        public double? Ceiling { get; set; }

        [JsonPropertyName("Floor")]
        public double? Floor { get; set; }

        [JsonPropertyName("RefPrice")]
        public double? RefPrice { get; set; }

        [JsonPropertyName("AvgPrice")]
        public double? AvgPrice { get; set; }

        [JsonPropertyName("PriorVal")]
        public double? PriorVal { get; set; }

        [JsonPropertyName("LastPrice")]
        public double? LastPrice { get; set; }

        [JsonPropertyName("LastVol")]
        public double? LastVol { get; set; }

        [JsonPropertyName("TotalVal")]
        public double? TotalVal { get; set; }

        [JsonPropertyName("TotalVol")]
        public double? TotalVol { get; set; }

        [JsonPropertyName("MarketId")]
        public string? MarketId { get; set; }

        [JsonPropertyName("Exchange")]
        public string? Exchange { get; set; }

        [JsonPropertyName("TradingSession")]
        public string? TradingSession { get; set; }

        [JsonPropertyName("TradingStatus")]
        public string? TradingStatus { get; set; }

        [JsonPropertyName("Change")]
        public double? Change { get; set; }

        [JsonPropertyName("RatioChange")]
        public double? RatioChange { get; set; }

        [JsonPropertyName("EstMatchedPrice")]
        public double? EstMatchedPrice { get; set; }

        [JsonPropertyName("Highest")]
        public double? Highest { get; set; }

        [JsonPropertyName("Lowest")]
        public double? Lowest { get; set; }

        [JsonPropertyName("Side")]
        public string? Side { get; set; }
    }

    public class ForeignRoomResponse
    {
        [JsonPropertyName("TradingDate")]
        public string? TradingDate { get; set; }

        [JsonPropertyName("Time")]
        public string? Time { get; set; }

        [JsonPropertyName("Isin")]
        public string? Isin { get; set; }

        [JsonPropertyName("Symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("TotalRoom")]
        public double? TotalRoom { get; set; }

        [JsonPropertyName("CurrentRoom")]
        public double? CurrentRoom { get; set; }

        [JsonPropertyName("BuyVol")]
        public double? FBuyVol { get; set; }

        [JsonPropertyName("BuyVal")]
        public double? FBuyVal { get; set; }

        [JsonPropertyName("SellVol")]
        public double? FSellVol { get; set; }

        [JsonPropertyName("SellVal")]
        public double? FSellVal { get; set; }

        [JsonPropertyName("MarketId")]
        public string? MarketId { get; set; }

        [JsonPropertyName("Exchange")]
        public string? Exchange { get; set; }

    }

    public class SecuritiesSnapshot
    {
        [JsonPropertyName("RType")]
        public string RType { get; set; }

        [JsonPropertyName("TradingDate")]
        public string TradingDate { get; set; }

        [JsonPropertyName("Time")]
        public string Time { get; set; }

        [JsonPropertyName("Isin")]
        public string Isin { get; set; }

        [JsonPropertyName("Symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("Ceiling")]
        public decimal Ceiling { get; set; }

        [JsonPropertyName("Floor")]
        public decimal Floor { get; set; }

        [JsonPropertyName("RefPrice")]
        public decimal RefPrice { get; set; }

        [JsonPropertyName("Open")]
        public decimal Open { get; set; }

        [JsonPropertyName("Close")]
        public decimal Close { get; set; }

        [JsonPropertyName("High")]
        public decimal High { get; set; }

        [JsonPropertyName("Low")]
        public decimal Low { get; set; }

        [JsonPropertyName("AvgPrice")]
        public decimal Avg { get; set; }

        [JsonPropertyName("PriorVal")]
        public decimal PriorVal { get; set; }

        [JsonPropertyName("LastPrice")]
        public decimal LastVal { get; set; }

        [JsonPropertyName("LastVol")]
        public decimal LastVol { get; set; }

        [JsonPropertyName("TotalVal")]
        public decimal TotalVal { get; set; }

        [JsonPropertyName("TotalVol")]
        public decimal TotalVol { get; set; }

        [JsonPropertyName("BidPrice1")]
        public decimal BidPrice1 { get; set; }
        [JsonPropertyName("BidPrice2")]
        public decimal BidPrice2 { get; set; }
        [JsonPropertyName("BidPrice3")]
        public decimal BidPrice3 { get; set; }
        [JsonPropertyName("BidPrice4")]
        public decimal BidPrice4 { get; set; }
        [JsonPropertyName("BidPrice5")]
        public decimal BidPrice5 { get; set; }
        [JsonPropertyName("BidPrice6")]
        public decimal BidPrice6 { get; set; }
        [JsonPropertyName("BidPrice7")]
        public decimal BidPrice7 { get; set; }
        [JsonPropertyName("BidPrice8")]
        public decimal BidPrice8 { get; set; }
        [JsonPropertyName("BidPrice9")]
        public decimal BidPrice9 { get; set; }
        [JsonPropertyName("BidPrice10")]
        public decimal BidPrice10 { get; set; }

        [JsonPropertyName("BidVol1")]
        public decimal BidVol1 { get; set; }
        [JsonPropertyName("BidVol2")]
        public decimal BidVol2 { get; set; }
        [JsonPropertyName("BidVol3")]
        public decimal BidVol3 { get; set; }
        [JsonPropertyName("BidVol4")]
        public decimal BidVol4 { get; set; }
        [JsonPropertyName("BidVol5")]
        public decimal BidVol5 { get; set; }
        [JsonPropertyName("BidVol6")]
        public decimal BidVol6 { get; set; }
        [JsonPropertyName("BidVol7")]
        public decimal BidVol7 { get; set; }
        [JsonPropertyName("BidVol8")]
        public decimal BidVol8 { get; set; }
        [JsonPropertyName("BidVol9")]
        public decimal BidVol9 { get; set; }
        [JsonPropertyName("BidVol10")]
        public decimal BidVol10 { get; set; }

        [JsonPropertyName("AskPrice1")]
        public decimal AskPrice1 { get; set; }
        [JsonPropertyName("AskPrice2")]
        public decimal AskPrice2 { get; set; }
        [JsonPropertyName("AskPrice3")]
        public decimal AskPrice3 { get; set; }
        [JsonPropertyName("AskPrice4")]
        public decimal AskPrice4 { get; set; }
        [JsonPropertyName("AskPrice5")]
        public decimal AskPrice5 { get; set; }
        [JsonPropertyName("AskPrice6")]
        public decimal AskPrice6 { get; set; }
        [JsonPropertyName("AskPrice7")]
        public decimal AskPrice7 { get; set; }
        [JsonPropertyName("AskPrice8")]
        public decimal AskPrice8 { get; set; }
        [JsonPropertyName("AskPrice9")]
        public decimal AskPrice9 { get; set; }
        [JsonPropertyName("AskPrice10")]
        public decimal AskPrice10 { get; set; }

        [JsonPropertyName("AskVol1")]
        public decimal AskVol1 { get; set; }
        [JsonPropertyName("AskVol2")]
        public decimal AskVol2 { get; set; }
        [JsonPropertyName("AskVol3")]
        public decimal AskVol3 { get; set; }
        [JsonPropertyName("AskVol4")]
        public decimal AskVol4 { get; set; }
        [JsonPropertyName("AskVol5")]
        public decimal AskVol5 { get; set; }
        [JsonPropertyName("AskVol6")]
        public decimal AskVol6 { get; set; }
        [JsonPropertyName("AskVol7")]
        public decimal AskVol7 { get; set; }
        [JsonPropertyName("AskVol8")]
        public decimal AskVol8 { get; set; }
        [JsonPropertyName("AskVol9")]
        public decimal AskVol9 { get; set; }
        [JsonPropertyName("AskVol10")]
        public decimal AskVol10 { get; set; }

        [JsonPropertyName("MarketId")]
        public string MarketId { get; set; }

        [JsonPropertyName("Exchange")]
        public string Exchange { get; set; }

        [JsonPropertyName("TradingSession")]
        public string TradingSession { get; set; }

        [JsonPropertyName("TradingStatus")]
        public string TradingStatus { get; set; }

        [JsonPropertyName("Change")]
        public decimal Change { get; set; }

        [JsonPropertyName("RatioChange")]
        public decimal RatioChange { get; set; }

        [JsonPropertyName("EstMatchedPrice")]
        public decimal EstMatchedPrice { get; set; }

        [JsonPropertyName("Side")]
        public string Side { get; set; }

        [JsonPropertyName("CloseQtty")]
        public decimal CloseQtty { get; set; }
    }
}
