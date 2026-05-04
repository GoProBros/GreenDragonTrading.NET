using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.CreatePaymentLink;

/// <summary>
/// Validator for CreatePaymentLinkCommand
/// </summary>
public class CreatePaymentLinkCommandValidator : AbstractValidator<CreatePaymentLinkCommand>
{
    public CreatePaymentLinkCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .GreaterThan(0).WithMessage("ID gói đăng ký phải lớn hơn 0");

        RuleFor(x => x.PaymentProvider)
            .IsInEnum().WithMessage("Phương thức thanh toán không hợp lệ. Chọn 1 (PayOS) hoặc 2 (Momo).");
    }
}
