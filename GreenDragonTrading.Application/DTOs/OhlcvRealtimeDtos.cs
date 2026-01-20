namespace GreenDragonTrading.Application.DTOs
{
    /// <summary>
    /// Represents a current (in-progress or just closed) OHLCV candle
    /// </summary>
    public class CurrentCandleDto
    {
        public string Ticker { get; set; } = string.Empty;
        public string Timeframe { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
        public decimal TotalValue { get; set; }
        public bool IsComplete { get; set; }
    }
}
