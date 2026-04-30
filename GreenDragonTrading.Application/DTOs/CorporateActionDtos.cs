namespace GreenDragonTrading.Application.DTOs;

public class CorporateActionDto
{
    public int EventId { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? TitleEvent { get; set; }
    public string? Content { get; set; }
    public string? Note { get; set; }
    public string? Url { get; set; }
    public DateTimeOffset? ExRightsDate { get; set; }
    public DateTimeOffset? RecordDate { get; set; }
    public DateTimeOffset? ActionDate { get; set; }
    public int EventType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
