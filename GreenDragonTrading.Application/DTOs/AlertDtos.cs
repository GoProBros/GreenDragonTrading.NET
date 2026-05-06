using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs
{
    public class AlertDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Ticker { get; set; } = string.Empty;
        public AlertType Type { get; set; }
        public ConditionType Condition { get; set; }
        public decimal? ChangePercentage { get; set; }
        public decimal? ThresholdValue { get; set; }
        public VolumeTimeFrame? VolumeTimeFrame { get; set; }
        public int? VolumeLookbackBars { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsTriggered { get; set; }
        public DateTimeOffset? LastTriggeredAt { get; set; }
        public int? ChatSessionId { get; set; }
        public string? MessageTemplate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
