namespace GreenDragonTrading.Application.DTOs
{
    public sealed class ProactiveAlertLayerBSettingsDto
    {
        public int Id { get; set; }
        public string Timeframe { get; set; } = string.Empty;
        public decimal MinAbsoluteMovePercent { get; set; }
        public decimal AtrMoveMultiplier { get; set; }
        public decimal MinVolumeRatio { get; set; }
        public decimal MinAdx { get; set; }
        public int MaxIndicatorSnapshotAgeMinutes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
