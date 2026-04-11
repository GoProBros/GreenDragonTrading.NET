using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.ToggleUserStatus;

/// <summary>
/// Validator for ToggleUserStatusCommand.
/// </summary>
public class ToggleUserStatusCommandValidator : AbstractValidator<ToggleUserStatusCommand>
{
    public ToggleUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Mã người dùng không hợp lệ.");
    }
}
