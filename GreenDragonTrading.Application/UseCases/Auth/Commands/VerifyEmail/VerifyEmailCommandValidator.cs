using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
    {
        public VerifyEmailCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Verification token là bắt buộc.")
                .MinimumLength(32).WithMessage("Định dạng verification token không hợp lệ");
        }
    }
}
