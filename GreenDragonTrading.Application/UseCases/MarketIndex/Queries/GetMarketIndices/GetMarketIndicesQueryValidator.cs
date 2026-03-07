using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetMarketIndices
{
    public class GetMarketIndicesQueryValidator : AbstractValidator<GetMarketIndicesQuery>
    {
        public GetMarketIndicesQueryValidator()
        {
            RuleFor(x => x.PageIndex)
                .GreaterThan(0)
                .WithMessage("PageIndex phải lớn hơn 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("PageSize phải nằm trong khoảng từ 1 đến 100");
        }
    }
}
