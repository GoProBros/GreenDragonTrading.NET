using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.DTOs
{
    public class SingleResponse<T>
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        [JsonPropertyName("status")]
        public int? Status { get; set; }
        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }

    public class AccessTokenResponse
    {
        [JsonPropertyName("accessToken")]
        public string? AccessToken { get; set; }
    }

    public class ResponseBase<T>
    {
        [DataMember(Order = 1, Name = "data")]
        public List<T> Data { get; set; } = [];

        [DataMember(Order = 2, Name = "message")]
        public string? Message { get; set; } = "Undefine";

        [DataMember(Order = 3, Name = "status")]
        public string? Status { get; set; } = "Undefine";

        [DataMember(Order = 4, Name = "totalRecord")]
        public int TotalRecord { get; set; } = 0;
    }

    #region SecuritiesDetailsResponse
    public class SecuritiesDetailsResponse : ResponseBase<SecuritiesDetailsResponseModel>
    {
    }
    public class SecuritiesDetailsResponseModel
    {
        [JsonPropertyName("RType")]
        public string? RType { get; set; }
        [JsonPropertyName("ReportDate")]
        public string? ReportDate { get; set; }
        [JsonPropertyName("TotalNoSym")]
        public string? TotalNoSym { get; set; }
        [JsonPropertyName("RepeatedInfo")]
        public List<RepeatedSecuritiesDetailsInfo> RepeatedInfo { get; set; } = [];

    }

    public class RepeatedSecuritiesDetailsInfo
    {
        [JsonPropertyName("Isin")]
        public string? Isin { get; set; }
        [JsonPropertyName("Symbol")]
        public string? Symbol { get; set; }
        [JsonPropertyName("SymbolName")]
        public string? SymbolName { get; set; }
        [JsonPropertyName("SymbolEngName")]
        public string? SymbolEngName { get; set; }
        [JsonPropertyName("SecType")]
        public string? SecType { get; set; }
        [JsonPropertyName("MarketId")]
        public string? MarketId { get; set; }
        [JsonPropertyName("Exchange")]
        public string? Exchange { get; set; }
        [JsonPropertyName("Issuer")]
        public string? Issuer { get; set; }
        [JsonPropertyName("LotSize")]
        public string? LotSize { get; set; }
        [JsonPropertyName("IssueDate")]
        public string? IssueDate { get; set; }
        [JsonPropertyName("MaturityDate")]
        public string? MaturityDate { get; set; }
        [JsonPropertyName("FirstTradingDate")]
        public string? FirstTradingDate { get; set; }
        [JsonPropertyName("LastTradingDate")]
        public string? LastTradingDate { get; set; }
        [JsonPropertyName("ContractMultiplier")]
        public string? ContractMultiplier { get; set; }
        [JsonPropertyName("SettlMethod")]
        public string? SettlMethod { get; set; }
        [JsonPropertyName("Underlying")]
        public string? Underlying { get; set; }
        [JsonPropertyName("PutOrCall")]
        public string? PutOrCall { get; set; }
        [JsonPropertyName("ExercisePrice")]
        public string? ExercisePrice { get; set; }
        [JsonPropertyName("ExerciseStyle")]
        public string? ExerciseStyle { get; set; }
        [JsonPropertyName("ExcerciseRatio")]
        public string? ExcerciseRatio { get; set; }
        [JsonPropertyName("ListedShare")]
        public string? ListedShare { get; set; }
        [JsonPropertyName("TickPrice1")]
        public string? TickPrice1 { get; set; }
        [JsonPropertyName("TickIncrement1")]
        public string? TickIncrement1 { get; set; }
        [JsonPropertyName("TickPrice2")]
        public string? TickPrice2 { get; set; }
        [JsonPropertyName("TickIncrement2")]
        public string? TickIncrement2 { get; set; }
        [JsonPropertyName("TickPrice3")]
        public string? TickPrice3 { get; set; }
        [JsonPropertyName("TickIncrement3")]
        public string? TickIncrement3 { get; set; }
        [JsonPropertyName("TickPrice4")]
        public string? TickPrice4 { get; set; }
        [JsonPropertyName("TickIncrement4")]
        public string? TickIncrement4 { get; set; }
    }
    #endregion SecuritiesDetailsResponse

    #region IntradayOhlcResponse
    public class IntradayOhlcResponse : ResponseBase<IntradayOhlcResponseModel>
    {
    }

    public class IntradayOhlcResponseModel
    {
        [JsonPropertyName("Symbol")]
        public string? Symbol { get; set; }
        [JsonPropertyName("Value")]
        public string? Value { get; set; }
        [JsonPropertyName("TradingDate")]
        public string? TradingDate { get; set; }
        [JsonPropertyName("Time")]
        public string? Time { get; set; }
        [JsonPropertyName("Open")]
        public string? Open { get; set; }
        [JsonPropertyName("High")]
        public string? High { get; set; }
        [JsonPropertyName("Low")]
        public string? Low { get; set; }
        [JsonPropertyName("Close")]
        public string? Close { get; set; }
        [JsonPropertyName("Volume")]
        public string? Volume { get; set; }
    }
    #endregion IntradayOhlcResponse

    #region DailyOhlcResponse
    public class DailyOhlcResponse : ResponseBase<DailyOhlcResponseModel>
    {
    }

    public class DailyOhlcResponseModel
    {
        [JsonPropertyName("Symbol")]
        public string? Symbol { get; set; }
        [JsonPropertyName("Market")]
        public string? Market { get; set; }
        [JsonPropertyName("TradingDate")]
        public string? TradingDate { get; set; }
        [JsonPropertyName("Time")]
        public string? Time { get; set; }
        [JsonPropertyName("Open")]
        public string? Open { get; set; }
        [JsonPropertyName("High")]
        public string? High { get; set; }
        [JsonPropertyName("Low")]
        public string? Low { get; set; }
        [JsonPropertyName("Close")]
        public string? Close { get; set; }
        [JsonPropertyName("Volume")]
        public string? Volume { get; set; }
        [JsonPropertyName("Value")]
        public string? Value { get; set; }
    }
    #endregion DailyOhlcResponse
}
