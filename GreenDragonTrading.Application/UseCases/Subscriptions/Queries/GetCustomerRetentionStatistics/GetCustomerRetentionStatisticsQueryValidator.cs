using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetCustomerRetentionStatistics;

/// <summary>
/// Validator for GetCustomerRetentionStatisticsQuery.
/// </summary>
public class GetCustomerRetentionStatisticsQueryValidator : AbstractValidator<GetCustomerRetentionStatisticsQuery>
{
    public GetCustomerRetentionStatisticsQueryValidator()
    {
    }
}
