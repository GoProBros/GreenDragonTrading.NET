namespace GreenDragonTrading.Application.DTOs
{
    /// <summary>
    /// Queue payload for asynchronous AI evaluation of proactive alert signals.
    /// </summary>
    public class ProactiveAiEvaluationJobDto
    {
        public string JobId { get; set; } = Guid.NewGuid().ToString("N");
        public List<Guid> TargetUserIds { get; set; } = new();
        public string Ticker { get; set; } = string.Empty;
        public DateTimeOffset EnqueuedAt { get; set; } = DateTimeOffset.UtcNow;

        // Layer A context
        public decimal ReferencePrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal CurrentVolume { get; set; }
        public decimal SignedMovePercent { get; set; }
        public string? TradingStatus { get; set; }
        public string? TradingSession { get; set; }

        // Layer B context
        public decimal RequiredMovePercent { get; set; }
        public decimal VolumeRatio { get; set; }
        public decimal Atr14 { get; set; }
        public decimal Adx14 { get; set; }
        public decimal Ema20 { get; set; }
        public decimal Ema50 { get; set; }
    }
}
