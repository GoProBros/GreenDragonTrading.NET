using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Commands.DeletePortfolio;

public class DeletePortfolioCommandValidator : AbstractValidator<DeletePortfolioCommand>
{
    public DeletePortfolioCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Portfolio ID phải lớn hơn 0");
    }
}
