using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Commands.UpdateSubscriptionStatus
{
    public class UpdateSubscriptionStatusCommandValidator : AbstractValidator<UpdateSubscriptionStatusCommand>
    {
        public UpdateSubscriptionStatusCommandValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .GreaterThan(0).WithMessage("Mã gói đăng ký không hợp lệ");

            RuleFor(x => x.IsActive)
                .IsInEnum().WithMessage("Trạng thái gói đăng ký không hợp lệ");
        }
    }
}
