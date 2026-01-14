using FluentValidation;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.CreateFinancialReport
{
    public class CreateFinancialReportCommandValidator : AbstractValidator<CreateFinancialReportCommand>
    {
        public CreateFinancialReportCommandValidator()
        {
            RuleFor(x => x.Ticker)
                .NotEmpty().WithMessage("Ticker không được để trống.")
                .MaximumLength(20).WithMessage("Ticker không được vượt quá 20 ký tự.");

            RuleFor(x => x.Year)
                .GreaterThan(2000).WithMessage("Năm phải lớn hơn 2000.")
                .LessThanOrEqualTo(DateTime.Now.Year + 1).WithMessage("Năm không được lớn hơn năm hiện tại.");

            RuleFor(x => x.Period)
                .IsInEnum().WithMessage("Kỳ báo cáo không hợp lệ.");
        }
    }
}
