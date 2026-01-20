namespace GreenDragonTrading.Application.DTOs
{
    /// <summary>
    /// Represents a single market tick (trade or quote update)
    /// Used for real-time OHLCV aggregation
    /// </summary>
    public class TickData
    {
        /// <summary>
        /// Stock ticker symbol
        /// </summary>
        public required string Ticker { get; set; }

        /// <summary>
        /// Price of the tick
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Volume of the tick
        /// </summary>
        public long Volume { get; set; }

        /// <summary>
        /// Value (Price * Volume)
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Timestamp of the tick (UTC)
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
