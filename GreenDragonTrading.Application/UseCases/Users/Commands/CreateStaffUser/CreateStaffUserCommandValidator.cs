using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.CreateStaffUser;

/// <summary>
/// Validator for CreateStaffUserCommand.
/// </summary>
public class CreateStaffUserCommandValidator : AbstractValidator<CreateStaffUserCommand>
{
    public CreateStaffUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email là bắt buộc.")
            .EmailAddress().WithMessage("Email không hợp lệ.")
            .MaximumLength(255).WithMessage("Email không được vượt quá 255 ký tự.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu là bắt buộc.")
            .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự.")
            .MaximumLength(100).WithMessage("Mật khẩu không được vượt quá 100 ký tự.")
            .Matches(@"[A-Z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ in hoa.")
            .Matches(@"[a-z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ thường.")
            .Matches(@"[0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ số.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Tên là bắt buộc.")
            .MaximumLength(24).WithMessage("Tên không được vượt quá 24 ký tự.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Số điện thoại là bắt buộc.")
            .Matches(@"^(\+84|0)[0-9]{9,10}$").WithMessage("Số điện thoại không hợp lệ. Định dạng: +84xxxxxxxxx hoặc 0xxxxxxxxx.");

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl))
            .WithMessage("AvatarUrl không được vượt quá 255 ký tự.");
    }
}