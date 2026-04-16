using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetWatchListTopInterestedSymbols;

/// <summary>
/// Validator for GetWatchListTopInterestedSymbolsQuery.
/// </summary>
public class GetWatchListTopInterestedSymbolsQueryValidator : AbstractValidator<GetWatchListTopInterestedSymbolsQuery>
{
    public GetWatchListTopInterestedSymbolsQueryValidator()
    {
    }
}
