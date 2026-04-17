using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Queries.GetPortfolios;

public class GetPortfoliosQueryValidator : AbstractValidator<GetPortfoliosQuery>
{
    public GetPortfoliosQueryValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThan(0)
            .WithMessage("PageIndex phải lớn hơn 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize phải nằm trong khoảng từ 1 đến 100");

        RuleFor(x => x.Ticker)
            .MaximumLength(20)
            .WithMessage("Ticker không được vượt quá 20 ký tự")
            .When(x => !string.IsNullOrWhiteSpace(x.Ticker));
    }
}
