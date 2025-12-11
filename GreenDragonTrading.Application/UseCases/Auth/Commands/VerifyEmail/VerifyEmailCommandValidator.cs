using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
    {
        public VerifyEmailCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Verification token is required")
                .MinimumLength(32).WithMessage("Invalid verification token format");
        }
    }
}
