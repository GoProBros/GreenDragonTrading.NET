using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required")
                .MinimumLength(32).WithMessage("Invalid refresh token format");
        }
    }
}
