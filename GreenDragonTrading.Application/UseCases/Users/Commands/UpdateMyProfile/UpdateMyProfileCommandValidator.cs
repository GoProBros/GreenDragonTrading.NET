using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.UpdateMyProfile;

/// <summary>
/// Validator for UpdateMyProfileCommand.
/// </summary>
public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => x.FullName != null || x.PhoneNumber != null || x.AvatarUrl != null)
            .WithMessage("Phải cung cấp ít nhất một trường để cập nhật.");

        RuleFor(x => x.FullName)
            .NotEmpty().When(x => x.FullName != null).WithMessage("Tên không được để trống.")
            .MaximumLength(24).When(x => !string.IsNullOrWhiteSpace(x.FullName)).WithMessage("Tên không được vượt quá 24 ký tự.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().When(x => x.PhoneNumber != null).WithMessage("Số điện thoại không được để trống.")
            .Matches(@"^(\+84|0)[0-9]{9,10}$").When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Số điện thoại không hợp lệ. Định dạng: +84xxxxxxxxx hoặc 0xxxxxxxxx.");

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl))
            .WithMessage("AvatarUrl không được vượt quá 255 ký tự.");
    }
}
