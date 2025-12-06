using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.DTOs
{
//    2025-12-03 09:15:12.307 +07:00 [INF] Broadcast received: {"DataType":"X-QUOTE","Content":"{\"TradingDate\":\"03/12/2025\",\"Time\":\"09:15:01\",\"Exchange\":\"HOSE\",\"Symbol\":\"SSI\",\"RType\":\"X-QUOTE\",\"AskPrice1\":32400.0,\"AskPrice2\":32450.0,\"AskPrice3\":32500.0,\"AskPrice4\":0.0,\"AskPrice5\":0.0,\"AskPrice6\":0.0,\"AskPrice7\":0.0,\"AskPrice8\":0.0,\"AskPrice9\":0.0,\"AskPrice10\":0.0,\"AskVol1\":60500.0,\"AskVol2\":30000.0,\"AskVol3\":43800.0,\"AskVol4\":0.0,\"AskVol5\":0.0,\"AskVol6\":0.0,\"AskVol7\":0.0,\"AskVol8\":0.0,\"AskVol9\":0.0,\"AskVol10\":0.0,\"BidPrice1\":32350.0,\"BidPrice2\":32300.0,\"BidPrice3\":32250.0,\"BidPrice4\":0.0,\"BidPrice5\":0.0,\"BidPrice6\":0.0,\"BidPrice7\":0.0,\"BidPrice8\":0.0,\"BidPrice9\":0.0,\"BidPrice10\":0.0,\"BidVol1\":62600.0,\"BidVol2\":138900.0,\"BidVol3\":91000.0,\"BidVol4\":0.0,\"BidVol5\":0.0,\"BidVol6\":0.0,\"BidVol7\":0.0,\"BidVol8\":0.0,\"BidVol9\":0.0,\"BidVol10\":0.0,\"TradingSession\":\"LO\"}"}

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
}
