using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs
{
    public sealed class AlertTemplateDto
    {
        public int Id { get; set; }
        public AlertType? Type { get; set; }
        public ConditionType? Condition { get; set; }
        public string TitleTemplate { get; set; } = string.Empty;
        public string BodyTemplate { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public sealed class AlertTemplatePlaceholdersDto
    {
        public List<AlertTemplatePlaceholderDto> Placeholders { get; set; } = [];
    }

    public sealed class AlertTemplatePlaceholderDto
    {
        public string Key { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
