using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Logout
{
    public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token không được để trống")
                .MinimumLength(32).WithMessage("Refresh token không hợp lệ");

            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("Access token không được để trống")
                .MinimumLength(20).WithMessage("Access token không hợp lệ");
        }
    }
}
