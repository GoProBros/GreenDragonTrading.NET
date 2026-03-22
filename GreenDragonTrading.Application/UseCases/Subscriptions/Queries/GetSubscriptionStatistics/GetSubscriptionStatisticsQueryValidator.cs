using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetSubscriptionStatistics;

/// <summary>
/// Validator for GetSubscriptionStatisticsQuery.
/// </summary>
public class GetSubscriptionStatisticsQueryValidator : AbstractValidator<GetSubscriptionStatisticsQuery>
{
    public GetSubscriptionStatisticsQueryValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100)
            .When(x => x.Year.HasValue)
            .WithMessage("Năm phải nằm trong khoảng từ 2000 đến 2100.");
    }
}