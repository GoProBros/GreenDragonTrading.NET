using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Commands.DeleteReport;

/// <summary>
/// Validator for DeleteReportCommand
/// </summary>
public class DeleteReportCommandValidator : AbstractValidator<DeleteReportCommand>
{
    public DeleteReportCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID là bắt buộc");
    }
}
