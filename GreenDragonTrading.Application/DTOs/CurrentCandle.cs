namespace GreenDragonTrading.Application.DTOs
{
    /// <summary>
    /// Represents an in-progress OHLCV candle being aggregated from ticks
    /// </summary>
    public class CurrentCandle
    {
        public required string Ticker { get; set; }
        public required string Timeframe { get; set; }
        public DateTime StartTime { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// Update candle with new tick data
        /// </summary>
        public void UpdateWithTick(decimal price, long volume, decimal value, DateTime timestamp)
        {
            // First tick initializes Open
            if (Volume == 0)
            {
                Open = price;
                High = price;
                Low = price;
                Close = price;
            }
            else
            {
                // Update High/Low
                if (price > High) High = price;
                if (price < Low) Low = price;
                Close = price;
            }

            Volume += volume;
            TotalValue += value;
            LastUpdateTime = timestamp;
        }

        /// <summary>
        /// Check if candle should be closed based on current timestamp
        /// </summary>
        public bool ShouldClose(DateTime currentTime)
        {
            var nextCandleTime = Timeframe.ToUpper() switch
            {
                "M1" => StartTime.AddMinutes(1),
                "M5" => StartTime.AddMinutes(5),
                "M15" => StartTime.AddMinutes(15),
                "M30" => StartTime.AddMinutes(30),
                "H1" => StartTime.AddHours(1),
                "H4" => StartTime.AddHours(4),
                "D1" => StartTime.AddDays(1),
                "W1" => StartTime.AddDays(7), // Weekly: 7 days
                "MN1" => StartTime.AddMonths(1), // Monthly: 1 month
                _ => StartTime.AddMinutes(1)
            };

            return currentTime >= nextCandleTime;
        }

        /// <summary>
        /// Convert to DTO for broadcasting
        /// </summary>
        public CurrentCandleDto ToDto(bool isComplete)
        {
            return new CurrentCandleDto
            {
                Ticker = Ticker,
                Timeframe = Timeframe,
                StartTime = StartTime,
                Open = Open,
                High = High,
                Low = Low,
                Close = Close,
                Volume = Volume,
                TotalValue = TotalValue,
                LastUpdateTime = LastUpdateTime,
                IsComplete = isComplete
            };
        }
    }
}
