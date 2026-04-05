using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.UpdateMyProfile;

/// <summary>
/// Validator for UpdateMyProfileCommand.
/// </summary>
public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Tên là bắt buộc.")
            .MaximumLength(24).WithMessage("Tên không được vượt quá 24 ký tự.");

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl))
            .WithMessage("AvatarUrl không được vượt quá 255 ký tự.");
    }
}
