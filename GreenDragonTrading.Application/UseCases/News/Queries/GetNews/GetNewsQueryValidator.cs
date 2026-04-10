using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.News.Queries.GetNews;

public class GetNewsQueryValidator : AbstractValidator<GetNewsQuery>
{
    public GetNewsQueryValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThan(0)
            .WithMessage("PageIndex phải lớn hơn 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize phải nằm trong khoảng từ 1 đến 100");

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .WithMessage("Search không được vượt quá 200 ký tự")
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.Ticker)
            .MaximumLength(20)
            .WithMessage("Ticker không được vượt quá 20 ký tự")
            .When(x => !string.IsNullOrWhiteSpace(x.Ticker));
    }
}
