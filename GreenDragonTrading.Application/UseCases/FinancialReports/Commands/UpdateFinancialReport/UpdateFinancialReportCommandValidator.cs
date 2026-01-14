using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.UpdateFinancialReport
{
    public class UpdateFinancialReportCommandValidator : AbstractValidator<UpdateFinancialReportCommand>
    {
        public UpdateFinancialReportCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID không được để trống.");

            RuleFor(x => x.Status)
                .IsInEnum().When(x => x.Status.HasValue)
                .WithMessage("Trạng thái không hợp lệ.");
        }
    }
}
