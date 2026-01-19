using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs
{
    public class SubscriptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SubscriptionLevel LevelOrder { get; set; }
        public int MaxLayouts { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
    }

    public class CreateSubscriptionRequest
    {
        public string Name { get; set; } = string.Empty;
        public SubscriptionLevel LevelOrder { get; set; }
        public int MaxLayouts { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
    }
}
