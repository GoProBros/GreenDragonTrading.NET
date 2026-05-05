using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token là bắt buộc.")
                .MinimumLength(32).WithMessage("Refresh token không hợp lệ");
        }
    }
}
