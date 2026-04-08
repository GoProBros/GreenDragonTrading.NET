using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReportIndicators
{
    public class GetFinancialReportIndicatorsQueryValidator : AbstractValidator<GetFinancialReportIndicatorsQuery>
    {
        public GetFinancialReportIndicatorsQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID không được để trống.");
        }
    }
}
