using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.ToggleAlertTemplateStatus;

public class ToggleAlertTemplateStatusCommandValidator : AbstractValidator<ToggleAlertTemplateStatusCommand>
{
    public ToggleAlertTemplateStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id template Không hợp lệ.");
    }
}
