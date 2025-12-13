using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.DTOs
{
    #region Finsc Request Models 

    /// <summary>    
    /// Request model for Finsc Stock API
    /// </summary>

    public class FinscStockRequest
    {
        /// <summary>
        /// Stock symbol (e.g., FPT, VNM)
        /// </summary>
        public string Symbol { get; set; }
        /// <summary>
        /// Chart Resolution: 1D (daily), 1H (hourly), 15 (15min),, 5 (5min), 1 (1min)
        /// </summary>
        public string Resolution { get; set; }
        /// <summary>
        /// From timestamp (Unix timestamp in seconds)
        /// </summary>
        public long From { get; set; }
        /// <summary>
        /// To timstamp (Unix timestamp in seconds)
        /// </summary>
        public long To { get; set; }

    }
    #endregion

    #region Finsc Response Models
    /// <summary>
    /// Response from Finsc API 
    /// </summary>
    public class FinscStockResonse
    {
        /// <summary>
        /// Unix timestamp (seconds)
        /// </summary>
        [JsonPropertyName("t")]
        public long[]? Timestamps { get; set; }
        /// <summary>
        /// Open prices
        /// </summary>
        [JsonPropertyName("0")]
        public decimal[]? Open { get; set; }
        /// <summary>
        /// High prices
        /// </summary>
        [JsonPropertyName("h")]
        public decimal[]? High { get; set; }
        /// <summary>
        /// Low prices
        /// </summary>
        [JsonPropertyName("l")]
        public decimal[]? Low { get; set; }
        /// <summary>
        /// Close prices
        /// </summary>
        [JsonPropertyName("c")]
        public decimal[]? Close { get; set; }
        /// <summary>
        /// Volumes
        /// </summary>
        [JsonPropertyName("v")]
        public decimal[]? Volume { get; set; }
        /// <summary>
        /// Symbol ticker
        /// </summary>
        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }
        /// <summary>
        /// Status: "ok" or "error"
        /// </summary>
        [JsonPropertyName("s")]
        public string? Status { get; set; }

    }
    #endregion

}
