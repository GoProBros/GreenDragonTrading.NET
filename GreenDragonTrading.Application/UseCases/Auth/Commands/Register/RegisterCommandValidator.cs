using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email là bắt buộc.")
                .EmailAddress().WithMessage("Email không hợp lệ.")
                .MaximumLength(255).WithMessage("Email không được vượt quá 255 ký tự.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu là bắt buộc.")
                .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự.")
                .MaximumLength(100).WithMessage("Mật khẩu không được vượt quá 100 ký tự.")
                .Matches(@"[A-Z]").WithMessage("Mật khẩu phải chứa ít nhất một chữ cái viết hoa.")
                .Matches(@"[a-z]").WithMessage("Mật khẩu phải chứa ít nhất một chữ cái viết thường.")
                .Matches(@"[0-9]").WithMessage("Mật khẩu phải chứa ít nhất một chữ số.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ và tên là bắt buộc.")
                .MaximumLength(255).WithMessage("Họ và tên không được vượt quá 255 ký tự.")
                .Matches(@"^[\p{L}\s]+$").WithMessage("Họ và tên chỉ được chứa chữ cái và khoảng trắng.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Số điện thoại là bắt buộc.")
                .Matches(@"^(\+84|0)[0-9]{9,10}$").WithMessage("Số điện thoại không hợp lệ. Phải theo định dạng Việt Nam (+84xxxxxxxxx hoặc 0xxxxxxxxx)");
        }
    }
}
