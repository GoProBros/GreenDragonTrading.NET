using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.ActivateUser;

/// <summary>
/// Validator for ActivateUserCommand.
/// </summary>
public class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId không hợp lệ.");
    }
}
