using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.DeleteFinancialReport
{
    public class DeleteFinancialReportCommandValidator : AbstractValidator<DeleteFinancialReportCommand>
    {
        public DeleteFinancialReportCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID không được để trống.");
        }
    }
}
