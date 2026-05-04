using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetRecentQuarterIndicators
{
    public class GetRecentQuarterIndicatorsQueryValidator : AbstractValidator<GetRecentQuarterIndicatorsQuery>
    {
        public GetRecentQuarterIndicatorsQueryValidator()
        {
            RuleFor(x => x.Ticker)
                .NotEmpty().WithMessage("Ticker không được để trống.")
                .MaximumLength(20).WithMessage("Ticker không được vượt quá 20 ký tự.");

            RuleFor(x => x.Count)
                .GreaterThan(0).WithMessage("Count phải lớn hơn 0.")
                .LessThanOrEqualTo(20).WithMessage("Count không được vượt quá 20.");
        }
    }
}
