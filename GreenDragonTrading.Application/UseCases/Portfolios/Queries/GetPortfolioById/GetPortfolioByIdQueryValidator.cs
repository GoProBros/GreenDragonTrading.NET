using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Queries.GetPortfolioById;

public class GetPortfolioByIdQueryValidator : AbstractValidator<GetPortfolioByIdQuery>
{
    public GetPortfolioByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Portfolio ID phải lớn hơn 0");
    }
}
