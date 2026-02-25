using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Queries.GetWatchListById;

/// <summary>
/// Validator for GetWatchListByIdQuery
/// </summary>
public class GetWatchListByIdQueryValidator : AbstractValidator<GetWatchListByIdQuery>
{
    public GetWatchListByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Watchlist ID phải lớn hơn 0");
    }
}
