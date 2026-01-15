using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReportById
{
    public class GetFinancialReportByIdQueryValidator : AbstractValidator<GetFinancialReportByIdQuery>
    {
        public GetFinancialReportByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID không được để trống.");
        }
    }
}
