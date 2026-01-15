using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReportsByTicker
{
    public class GetFinancialReportsByTickerQueryValidator : AbstractValidator<GetFinancialReportsByTickerQuery>
    {
        public GetFinancialReportsByTickerQueryValidator()
        {
            RuleFor(x => x.Ticker)
                .NotEmpty().WithMessage("Ticker không được để trống.")
                .MaximumLength(20).WithMessage("Ticker không được vượt quá 20 ký tự.");

            RuleFor(x => x.PageIndex)
                .GreaterThan(0).WithMessage("PageIndex phải lớn hơn 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("PageSize phải lớn hơn 0.")
                .LessThanOrEqualTo(100).WithMessage("PageSize không được vượt quá 100.");
        }
    }
}
