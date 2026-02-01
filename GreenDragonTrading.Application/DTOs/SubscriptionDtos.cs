using GreenDragonTrading.Domain.Enums;
using System.Text.Json;

namespace GreenDragonTrading.Application.DTOs
{
    public class SubscriptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SubscriptionLevel LevelOrder { get; set; }
        public int MaxWorkspaces { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public JsonElement AllowedModules { get; set; }
    }

    public class CreateSubscriptionRequest
    {
        public string Name { get; set; } = string.Empty;
        public SubscriptionLevel LevelOrder { get; set; }
        public int MaxWorkspaces { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public JsonElement AllowedModules { get; set; }
    }

    public class UserSubscriptionDto
    {
        public int? SubscriptionId { get; set; }
        public string SubscriptionName { get; set; } = string.Empty;
        public SubscriptionLevel LevelOrder { get; set; }
        public int MaxWorkspaces { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public JsonElement AllowedModules { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string? Status { get; set; }
        public bool IsActive { get; set; }
    }
}
