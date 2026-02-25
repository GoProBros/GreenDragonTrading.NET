using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Commands.DeleteWatchList;

/// <summary>
/// Validator for DeleteWatchListCommand
/// </summary>
public class DeleteWatchListCommandValidator : AbstractValidator<DeleteWatchListCommand>
{
    public DeleteWatchListCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Watchlist ID phải lớn hơn 0");
    }
}
