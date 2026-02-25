using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Commands.DeleteLayout;

/// <summary>
/// Validator for DeleteLayoutCommand
/// </summary>
public class DeleteLayoutCommandValidator : AbstractValidator<DeleteLayoutCommand>
{
    public DeleteLayoutCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Layout ID phải lớn hơn 0");
    }
}
