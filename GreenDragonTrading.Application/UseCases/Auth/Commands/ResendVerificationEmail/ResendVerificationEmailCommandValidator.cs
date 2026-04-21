using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.ResendVerificationEmail
{
    public class ResendVerificationEmailCommandValidator : AbstractValidator<ResendVerificationEmailCommand>
    {
        public ResendVerificationEmailCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email is invalid")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters");
        }
    }
}
