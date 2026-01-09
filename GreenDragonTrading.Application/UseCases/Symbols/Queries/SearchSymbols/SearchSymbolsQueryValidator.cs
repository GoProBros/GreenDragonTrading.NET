using FluentValidation;
using GreenDragonTrading.Application.Common.Validation;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.SearchSymbols
{
    public class SearchSymbolsQueryValidator : AbstractValidator<SearchSymbolsQuery>
    {
        public SearchSymbolsQueryValidator()
        {
            RuleFor(x => x.PageIndex)
                .GreaterThan(0)
                .WithMessage("PageIndex phải lớn hơn 0");

            RuleFor(x => x.PageSize)
                .ValidPageSize();

            RuleFor(x => x.Query)
                .MaximumLength(255)
                .WithMessage("Query không được vượt quá 255 ký tự");
        }
    }
}
