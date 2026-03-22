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
        var year = request.Year ?? DateTimeOffset.UtcNow.Year;
        var now = DateTimeOffset.UtcNow;

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

        var newUsersByMonth = Enumerable.Repeat(0, 12).ToList();
        foreach (var user in endUsers.Where(u => u.CreatedAt.Year == year))
        {
            newUsersByMonth[user.CreatedAt.Month - 1]++;
        }

        var revenueByMonth = Enumerable.Repeat(0m, 12).ToList();
        foreach (var transaction in completedTransactions.Where(t => t.CreatedAt.Year == year))
        {
            revenueByMonth[transaction.CreatedAt.Month - 1] += transaction.Amount;
        }

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
            Year = year,
            TotalUsers = endUsers.Count,
            ActiveUsers = endUsers.Count(x => x.Status == CommonStatus.Active),
            InactiveUsers = endUsers.Count(x => x.Status == CommonStatus.InActive),
            TotalRevenue = completedTransactions.Sum(x => x.Amount),
            NewUsersByMonth = newUsersByMonth,
            RevenueByMonth = revenueByMonth,
            CurrentUsersByVipLevel = currentUsersByVipLevel,
            VipPackageUsages = vipPackageUsages
        };

        _logger.LogInformation(
            "Subscription statistics retrieved successfully for year {Year}: TotalUsers={TotalUsers}, TotalRevenue={TotalRevenue}",
            year,
            result.TotalUsers,
            result.TotalRevenue);

        return ApiResponse<SubscriptionStatisticsDto>.Success(result, "Lấy thống kê thành công.");
    }
}