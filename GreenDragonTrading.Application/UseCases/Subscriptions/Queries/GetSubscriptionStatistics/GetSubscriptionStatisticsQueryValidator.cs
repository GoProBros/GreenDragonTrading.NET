using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetSubscriptionStatistics;

/// <summary>
/// Validator for GetSubscriptionStatisticsQuery.
/// </summary>
public class GetSubscriptionStatisticsQueryValidator : AbstractValidator<GetSubscriptionStatisticsQuery>
{
    public GetSubscriptionStatisticsQueryValidator()
    {
    }
}