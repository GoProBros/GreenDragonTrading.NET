using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống")
                .EmailAddress().WithMessage("Email không hợp lệ");

            RuleFor(x => x.ResetToken)
                .NotEmpty().WithMessage("Reset token không được để trống")
                .Length(6).WithMessage("Reset token phải có 6 ký tự");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Mật khẩu mới không được để trống")
                .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự")
                .Matches(@"[A-Z]").WithMessage("Mật khẩu phải có ít nhất 1 chữ hoa")
                .Matches(@"[a-z]").WithMessage("Mật khẩu phải có ít nhất 1 chữ thường")
                .Matches(@"[0-9]").WithMessage("Mật khẩu phải có ít nhất 1 chữ số");
        }
    }
}
