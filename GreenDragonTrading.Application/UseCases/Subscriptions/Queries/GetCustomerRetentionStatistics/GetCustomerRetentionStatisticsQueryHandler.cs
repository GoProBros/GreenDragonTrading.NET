using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetCustomerRetentionStatistics;

/// <summary>
/// Handler for GetCustomerRetentionStatisticsQuery.
/// </summary>
public class GetCustomerRetentionStatisticsQueryHandler : IRequestHandler<GetCustomerRetentionStatisticsQuery, ApiResponse<CustomerRetentionStatisticsDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<GetCustomerRetentionStatisticsQueryHandler> _logger;

    public GetCustomerRetentionStatisticsQueryHandler(
        IUnitOfWork uow,
        ILogger<GetCustomerRetentionStatisticsQueryHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<ApiResponse<CustomerRetentionStatisticsDto>> Handle(
        GetCustomerRetentionStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var activeCustomerIds = (await _uow.Users.GetActiveUsersAsync(cancellationToken))
            .Where(user => user.Role == UserRole.User)
            .Select(user => user.Id)
            .ToHashSet();

        var subscriptions = await _uow.UserSubscriptions.GetAllAsync(cancellationToken);

        var registrationsByCustomer = subscriptions
            .Where(subscription => activeCustomerIds.Contains(subscription.UserId))
            .GroupBy(subscription => subscription.UserId)
            .ToDictionary(group => group.Key, group => group.Count());

        var atLeast1 = registrationsByCustomer.Count(pair => pair.Value >= 1);
        var atLeast3 = registrationsByCustomer.Count(pair => pair.Value >= 3);
        var atLeast6 = registrationsByCustomer.Count(pair => pair.Value >= 6);

        var result = new CustomerRetentionStatisticsDto
        {
            TotalActiveCustomers = activeCustomerIds.Count,
            CustomersRegisteredAtLeast1Time = atLeast1,
            CustomersRegisteredAtLeast3Times = atLeast3,
            CustomersRegisteredAtLeast6Times = atLeast6,
            CustomersRegisteredAtLeast1TimeRate = CalculateRate(atLeast1, activeCustomerIds.Count),
            CustomersRegisteredAtLeast3TimesRate = CalculateRate(atLeast3, activeCustomerIds.Count),
            CustomersRegisteredAtLeast6TimesRate = CalculateRate(atLeast6, activeCustomerIds.Count)
        };

        _logger.LogInformation(
            "Customer retention statistics retrieved: TotalActiveCustomers={TotalActiveCustomers}, AtLeast1={AtLeast1}, AtLeast3={AtLeast3}, AtLeast6={AtLeast6}",
            result.TotalActiveCustomers,
            result.CustomersRegisteredAtLeast1Time,
            result.CustomersRegisteredAtLeast3Times,
            result.CustomersRegisteredAtLeast6Times);

        return ApiResponse<CustomerRetentionStatisticsDto>.Success(result, "Lấy thống kê giữ chân khách hàng thành công.");
    }

    private static decimal CalculateRate(int count, int total)
    {
        if (total == 0)
        {
            return 0m;
        }

        return Math.Round(count * 100m / total, 2);
    }
}
