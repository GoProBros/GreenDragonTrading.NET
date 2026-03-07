using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetIndexConstituents
{
    public class GetIndexConstituentsQueryValidator : AbstractValidator<GetIndexConstituentsQuery>
    {
        public GetIndexConstituentsQueryValidator()
        {
            RuleFor(x => x.IndexCode)
                .NotEmpty()
                .WithMessage("IndexCode không được để trống")
                .MaximumLength(20)
                .WithMessage("IndexCode tối đa 20 ký tự");

            RuleFor(x => x.PageIndex)
                .GreaterThan(0)
                .WithMessage("PageIndex phải lớn hơn 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("PageSize phải nằm trong khoảng từ 1 đến 100");
        }
    }
}
