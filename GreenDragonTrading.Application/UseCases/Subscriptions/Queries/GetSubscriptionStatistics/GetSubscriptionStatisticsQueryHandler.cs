using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetSubscriptionStatistics;

/// <summary>
/// Handler for GetSubscriptionStatisticsQuery.
/// </summary>
public class GetSubscriptionStatisticsQueryHandler : IRequestHandler<GetSubscriptionStatisticsQuery, ApiResponse<SubscriptionStatisticsDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<GetSubscriptionStatisticsQueryHandler> _logger;

    public GetSubscriptionStatisticsQueryHandler(
        IUnitOfWork uow,
        ILogger<GetSubscriptionStatisticsQueryHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<ApiResponse<SubscriptionStatisticsDto>> Handle(
        GetSubscriptionStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var firstMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(-11);
        var previousMonthStart = firstMonthStart.AddMonths(-1);
        var monthStarts = Enumerable.Range(0, 12)
            .Select(offset => firstMonthStart.AddMonths(offset))
            .ToList();
        var monthKeys = monthStarts
            .Select(start => $"{start.Year:D4}-{start.Month:D2}")
            .ToList();
        var lastMonthEndExclusive = firstMonthStart.AddMonths(12);

        var users = (await _uow.Users.GetAllAsync(cancellationToken)).ToList();
        var subscriptions = (await _uow.Subscriptions.GetAllAsync(cancellationToken))
            .OrderBy(s => s.LevelOrder)
            .ToList();
        var userSubscriptions = (await _uow.UserSubscriptions.GetAllAsync(cancellationToken)).ToList();
        var completedTransactions = (await _uow.Transactions.FindAsync(
            t => t.Status == TransactionStatus.Completed,
            cancellationToken)).ToList();

        var endUsers = users.Where(u => u.Role == UserRole.User).ToList();
        var vipSubscriptions = subscriptions
            .Where(s => s.LevelOrder != SubscriptionLevel.Free)
            .ToList();

        var newUsersByMonthMap = endUsers
            .Where(u => u.CreatedAt >= firstMonthStart && u.CreatedAt < lastMonthEndExclusive)
            .GroupBy(u => $"{u.CreatedAt.Year:D4}-{u.CreatedAt.Month:D2}")
            .ToDictionary(g => g.Key, g => g.Count());

        var revenueByMonthMap = completedTransactions
            .Where(t => t.CreatedAt >= previousMonthStart && t.CreatedAt < lastMonthEndExclusive)
            .GroupBy(t => $"{t.CreatedAt.Year:D4}-{t.CreatedAt.Month:D2}")
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        var newUsersByMonth = monthKeys
            .Select(key => newUsersByMonthMap.TryGetValue(key, out var count) ? count : 0)
            .ToList();

        var revenueByMonth = monthKeys
            .Select(key => revenueByMonthMap.TryGetValue(key, out var amount) ? amount : 0m)
            .ToList();

        var revenueGrowthPercentageByMonth = Enumerable.Range(0, revenueByMonth.Count)
            .Select(index =>
            {
                var previousRevenue = index == 0
                    ? (revenueByMonthMap.TryGetValue($"{previousMonthStart.Year:D4}-{previousMonthStart.Month:D2}", out var month13Revenue)
                        ? month13Revenue
                        : 0m)
                    : revenueByMonth[index - 1];
                var currentRevenue = revenueByMonth[index];

                if (previousRevenue == 0)
                {
                    return currentRevenue == 0 ? 0m : 100m;
                }

                return Math.Round((currentRevenue - previousRevenue) * 100 / previousRevenue, 2);
            })
            .ToList();

        var totalRevenue = completedTransactions.Sum(x => x.Amount);

        var revenuePercentageOfTotalByMonth = revenueByMonth
            .Select(monthRevenue => totalRevenue == 0
                ? 0m
                : Math.Round(monthRevenue * 100 / totalRevenue, 2))
            .ToList();

        var newUsersGrowthPercentageByMonth = newUsersByMonth
            .Select(newUsers => endUsers.Count == 0
                ? 0m
                : Math.Round((decimal)newUsers * 100 / endUsers.Count, 2))
            .ToList();

        var monthLabels = monthStarts
            .Select(start => $"{start.Month:D2}/{start.Year}")
            .ToList();

        var activeNowSubscriptions = userSubscriptions
            .Where(x => x.Status == SubscriptionStatus.Active && x.StartDate <= now && x.EndDate >= now)
            .ToList();

        var currentUsersByVipLevel = vipSubscriptions
            .Select(subscription => new VipCurrentUserCountDto
            {
                SubscriptionId = subscription.Id,
                SubscriptionName = subscription.Name,
                LevelOrder = subscription.LevelOrder,
                LevelDisplayName = subscription.LevelOrder.GetDisplayName(),
                UserCount = activeNowSubscriptions
                    .Where(x => x.SubscriptionId == subscription.Id)
                    .Select(x => x.UserId)
                    .Distinct()
                    .Count()
            })
            .ToList();

        var vipPackageUsages = vipSubscriptions
            .Select(subscription =>
            {
                var packageRegistrations = userSubscriptions
                    .Where(x => x.SubscriptionId == subscription.Id)
                    .ToList();

                var packageTransactions = completedTransactions
                    .Where(x => x.SubscriptionId == subscription.Id)
                    .ToList();

                return new VipPackageUsageDto
                {
                    SubscriptionId = subscription.Id,
                    SubscriptionName = subscription.Name,
                    LevelOrder = subscription.LevelOrder,
                    LevelDisplayName = subscription.LevelOrder.GetDisplayName(),
                    RegistrationCount = packageRegistrations.Count,
                    UniqueUserCount = packageRegistrations
                        .Select(x => x.UserId)
                        .Distinct()
                        .Count(),
                    TotalRevenue = packageTransactions.Sum(x => x.Amount)
                };
            })
            .OrderByDescending(x => x.RegistrationCount)
            .ThenBy(x => x.LevelOrder)
            .ToList();

        var result = new SubscriptionStatisticsDto
        {
            TotalUsers = endUsers.Count,
            ActiveUsers = endUsers.Count(x => x.Status == CommonStatus.Active),
            InactiveUsers = endUsers.Count(x => x.Status == CommonStatus.InActive),
            TotalRevenue = totalRevenue,
            MonthLabels = monthLabels,
            NewUsersByMonth = newUsersByMonth,
            RevenueByMonth = revenueByMonth,
            RevenueGrowthPercentageByMonth = revenueGrowthPercentageByMonth,
            RevenuePercentageOfTotalByMonth = revenuePercentageOfTotalByMonth,
            NewUsersGrowthPercentageByMonth = newUsersGrowthPercentageByMonth,
            CurrentUsersByVipLevel = currentUsersByVipLevel,
            VipPackageUsages = vipPackageUsages
        };

        _logger.LogInformation(
            "Subscription statistics retrieved successfully for rolling 12 months from {FromMonth} to {ToMonth}: TotalUsers={TotalUsers}, TotalRevenue={TotalRevenue}",
            monthLabels.First(),
            monthLabels.Last(),
            result.TotalUsers,
            result.TotalRevenue);

        return ApiResponse<SubscriptionStatisticsDto>.Success(result, "Lấy thống kê thành công.");
    }
}