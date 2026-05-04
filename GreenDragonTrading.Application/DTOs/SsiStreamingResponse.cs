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

        [JsonPropertyName("Open")]
        public double? Open { get; set; }

        [JsonPropertyName("Close")]
        public double? Close { get; set; }

        [JsonPropertyName("High")]
        public double? High { get; set; }

        [JsonPropertyName("Low")]
        public double? Low { get; set; }

        //[JsonPropertyName("AvgPrice")]
        //public double? Avg { get; set; }

        [JsonPropertyName("PriorVal")]
        public double? PriorVal { get; set; }

        [JsonPropertyName("LastPrice")]
        public double? LastVal { get; set; }

        [JsonPropertyName("LastVol")]
        public double? LastVol { get; set; }

        [JsonPropertyName("TotalVal")]
        public double? TotalVal { get; set; }

        [JsonPropertyName("TotalVol")]
        public double? TotalVol { get; set; }

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

        [JsonPropertyName("Side")]
        public string? Side { get; set; }

        [JsonPropertyName("CloseQtty")]
        public double? CloseQtty { get; set; }
    }

    /// <summary>
    /// Channel MI: Realtime market index data (VNINDEX, VN30, HNX30, …).
    /// SSI data type: "MI"
    /// </summary>
    public class MarketIndexDataResponse
    {
        [JsonPropertyName("RType")]
        public string? RType { get; set; }

        [JsonPropertyName("Rtype")]
        public string? Rtype { get; set; }

        /// <summary>
        /// SSI uses different field names for the index code depending on API version.
        /// Known variants: "Symbol", "ComGroupCode", "IndexCode", "Id".
        /// Use the resolved property <see cref="ResolvedCode"/> in handlers.
        /// </summary>
        [JsonPropertyName("Symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("ComGroupCode")]
        public string? ComGroupCode { get; set; }

        [JsonPropertyName("IndexCode")]
        public string? IndexCode { get; set; }

        [JsonPropertyName("IndexId")]
        public string? IndexID { get; set; }

        [JsonPropertyName("Id")]
        public string? Id { get; set; }

        /// <summary>Returns the first non-empty index code found across all candidate fields.</summary>
        [JsonIgnore]
        public string? ResolvedCode =>
            !string.IsNullOrWhiteSpace(IndexID)    ? IndexID    :
            !string.IsNullOrWhiteSpace(Symbol)      ? Symbol      :
            !string.IsNullOrWhiteSpace(ComGroupCode) ? ComGroupCode :
            !string.IsNullOrWhiteSpace(IndexCode)   ? IndexCode   :
            !string.IsNullOrWhiteSpace(Id)          ? Id          :
            null;

        [JsonPropertyName("IndexName")]
        public string? IndexName { get; set; }

        [JsonPropertyName("Exchange")]
        public string? Exchange { get; set; }

        [JsonPropertyName("TradingDate")]
        public string? TradingDate { get; set; }

        [JsonPropertyName("Time")]
        public string? Time { get; set; }

        [JsonPropertyName("IndexValue")]
        public double? IndexValue { get; set; }

        [JsonPropertyName("PriorIndexValue")]
        public double? PriorIndexValue { get; set; }

        [JsonPropertyName("IndexValEst")]
        public double? IndexValEst { get; set; }

        [JsonPropertyName("Change")]
        public double? Change { get; set; }

        [JsonPropertyName("RatioChange")]
        public double? RatioChange { get; set; }

        [JsonPropertyName("TotalTrade")]
        public double? TotalTrade { get; set; }

        [JsonPropertyName("TotalMatchVol")]
        public double? TotalMatchVol { get; set; }

        [JsonPropertyName("TotalQtty")]
        public double? TotalQtty { get; set; }

        [JsonPropertyName("TotalMatchVal")]
        public double? TotalMatchVal { get; set; }

        [JsonPropertyName("TotalValue")]
        public double? TotalValue { get; set; }

        [JsonPropertyName("AdvanceCount")]
        public int? AdvanceCount { get; set; }

        [JsonPropertyName("Advances")]
        public int? Advances { get; set; }

        [JsonPropertyName("DeclineCount")]
        public int? DeclineCount { get; set; }

        [JsonPropertyName("Declines")]
        public int? Declines { get; set; }

        [JsonPropertyName("NoChangeCount")]
        public int? NoChangeCount { get; set; }

        [JsonPropertyName("NoChanges")]
        public int? Nochanges { get; set; }

        [JsonPropertyName("TypeIndex")]
        public string? TypeIndex { get; set; }

        [JsonPropertyName("Ceiling")]
        public int? Ceiling { get; set; }

        [JsonPropertyName("Floor")]
        public int? Floor { get; set; }

        /// <summary>Number of stocks at ceiling price in the index basket (SSI MI channel sends "Ceilings" plural).</summary>
        [JsonPropertyName("Ceilings")]
        public int? Ceilings { get; set; }

        /// <summary>Number of stocks at floor price in the index basket (SSI MI channel sends "Floors" plural).</summary>
        [JsonPropertyName("Floors")]
        public int? Floors { get; set; }

        [JsonPropertyName("TotalQttyPt")]
        public double? TotalQttyPT { get; set; }

        [JsonPropertyName("TotalValuePt")]
        public double? TotalValuePT { get; set; }

        [JsonPropertyName("TotalQttyOd")]
        public double? TotalQttyOd { get; set; }

        [JsonPropertyName("TotalValueOd")]
        public double? TotalValueOd { get; set; }

        [JsonPropertyName("AllQty")]
        public double? AllQty { get; set; }

        [JsonPropertyName("AllValue")]
        public double? AllValue { get; set; }

        [JsonPropertyName("TradingSession")]
        public string? TradingSession { get; set; }

        [JsonPropertyName("RefIndex")]
        public double? RefIndex { get; set; }

        [JsonPropertyName("OpenIndex")]
        public double? OpenIndex { get; set; }

        [JsonPropertyName("HighIndex")]
        public double? HighIndex { get; set; }

        [JsonPropertyName("LowIndex")]
        public double? LowIndex { get; set; }

        [JsonIgnore]
        public int? ResolvedAdvanceCount => AdvanceCount ?? Advances;

        [JsonIgnore]
        public int? ResolvedDeclineCount => DeclineCount ?? Declines;

        [JsonIgnore]
        public int? ResolvedNoChangeCount => NoChangeCount ?? Nochanges;

        [JsonIgnore]
        public double? ResolvedTotalMatchVol => TotalMatchVol ?? TotalQtty ?? AllQty;

        [JsonIgnore]
        public double? ResolvedTotalMatchVal => TotalMatchVal ?? TotalValue ?? AllValue;

        [JsonIgnore]
        public string? ResolvedName => !string.IsNullOrWhiteSpace(IndexName) ? IndexName : null;
    }

    /// <summary>
    /// Channel B: Realtime OHLCV (Open, High, Low, Close, Volume) data
    /// </summary>
    public class OhlcvDataResponse
    {
        [JsonPropertyName("RType")]
        public string? RType { get; set; }

        [JsonPropertyName("Symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("TradingTime")]
        public string? TradingTime { get; set; }

        [JsonPropertyName("Open")]
        public double? Open { get; set; }

        [JsonPropertyName("High")]
        public double? High { get; set; }

        [JsonPropertyName("Low")]
        public double? Low { get; set; }

        [JsonPropertyName("Close")]
        public double? Close { get; set; }

        [JsonPropertyName("Volume")]
        public double? Volume { get; set; }

        [JsonPropertyName("Value")]
        public double? Value { get; set; }
    }
}
