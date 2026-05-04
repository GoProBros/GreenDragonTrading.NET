namespace GreenDragonTrading.Application.DTOs
{
    /// <summary>
    /// Latest raw indicator snapshot persisted in Redis.
    /// </summary>
    public class IndicatorSnapshotDto
    {
        public string Ticker { get; set; } = string.Empty;
        public string Timeframe { get; set; } = string.Empty;
        public DateTime CandleTime { get; set; }
        public decimal? Close { get; set; }
        public long? Volume { get; set; }
        public decimal? Ema20 { get; set; }
        public decimal? Ema50 { get; set; }
        public decimal? Ema200 { get; set; }
        public decimal? VolumeMa20 { get; set; }
        public decimal? VolumeToVolumeMa20Ratio { get; set; }
        public decimal? Macd { get; set; }
        public decimal? MacdSignal { get; set; }
        public decimal? MacdHistogram { get; set; }
        public decimal? Rsi14 { get; set; }
        public decimal? BollingerMiddle { get; set; }
        public decimal? BollingerUpper { get; set; }
        public decimal? BollingerLower { get; set; }
        public decimal? BbPercentB { get; set; }
        public decimal? Atr14 { get; set; }
        public decimal? Adx14 { get; set; }
        public DateTime CalculatedAt { get; set; }
    }

    /// <summary>
    /// Z-score normalized indicator snapshot built from the latest Redis snapshot.
    /// </summary>
    public class IndicatorSnapshotZScoreDto
    {
        public string Ticker { get; set; } = string.Empty;
        public string Timeframe { get; set; } = string.Empty;
        public DateTime CandleTime { get; set; }
        public DateTime CalculatedAt { get; set; }

        public decimal? Close { get; set; }
        public decimal? Volume { get; set; }
        public decimal? Ema20 { get; set; }
        public decimal? Ema50 { get; set; }
        public decimal? Ema200 { get; set; }
        public decimal? VolumeMa20 { get; set; }
        public decimal? VolumeToVolumeMa20Ratio { get; set; }
        public decimal? Macd { get; set; }
        public decimal? MacdSignal { get; set; }
        public decimal? MacdHistogram { get; set; }
        public decimal? Rsi14 { get; set; }
        public decimal? BollingerMiddle { get; set; }
        public decimal? BollingerUpper { get; set; }
        public decimal? BollingerLower { get; set; }
        public decimal? BbPercentB { get; set; }
        public decimal? Atr14 { get; set; }
        public decimal? Adx14 { get; set; }
    }
}