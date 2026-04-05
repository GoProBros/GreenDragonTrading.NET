using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Commands.UpdateSubscriptionPrice
{
    public class UpdateSubscriptionPriceCommandValidator : AbstractValidator<UpdateSubscriptionPriceCommand>
    {
        public UpdateSubscriptionPriceCommandValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .GreaterThan(0).WithMessage("Mã gói đăng ký không hợp lệ");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Giá không được âm");
        }
    }
}
