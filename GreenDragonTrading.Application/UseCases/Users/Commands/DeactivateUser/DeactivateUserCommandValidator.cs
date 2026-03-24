using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.DeactivateUser;

/// <summary>
/// Validator for DeactivateUserCommand.
/// </summary>
public class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId không hợp lệ.");
    }
}
