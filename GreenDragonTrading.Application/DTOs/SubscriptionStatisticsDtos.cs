using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs;

/// <summary>
/// Overall statistics response for users, subscriptions, and revenue.
/// </summary>
public class SubscriptionStatisticsDto
{
    public int Year { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<int> NewUsersByMonth { get; set; } = new();
    public List<decimal> RevenueByMonth { get; set; } = new();
    public List<VipCurrentUserCountDto> CurrentUsersByVipLevel { get; set; } = new();
    public List<VipPackageUsageDto> VipPackageUsages { get; set; } = new();
}

/// <summary>
/// Number of users currently active in each VIP package level.
/// </summary>
public class VipCurrentUserCountDto
{
    public int SubscriptionId { get; set; }
    public string SubscriptionName { get; set; } = string.Empty;
    public SubscriptionLevel LevelOrder { get; set; }
    public string LevelDisplayName { get; set; } = string.Empty;
    public int UserCount { get; set; }
}

/// <summary>
/// VIP package usage summary. Registration count includes all historical registrations,
/// including expired and upgraded subscriptions.
/// </summary>
public class VipPackageUsageDto
{
    public int SubscriptionId { get; set; }
    public string SubscriptionName { get; set; } = string.Empty;
    public SubscriptionLevel LevelOrder { get; set; }
    public string LevelDisplayName { get; set; } = string.Empty;
    public int RegistrationCount { get; set; }
    public int UniqueUserCount { get; set; }
    public decimal TotalRevenue { get; set; }
}