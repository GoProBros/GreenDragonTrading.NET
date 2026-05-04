using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.CreateMomoPaymentLink
{
    public class CreateMomoPaymentLinkCommandValidator : AbstractValidator<CreateMomoPaymentLinkCommand>
    {
        public CreateMomoPaymentLinkCommandValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .GreaterThan(0)
                .WithMessage("SubscriptionId phải lớn hơn 0.");
        }
    }
}
