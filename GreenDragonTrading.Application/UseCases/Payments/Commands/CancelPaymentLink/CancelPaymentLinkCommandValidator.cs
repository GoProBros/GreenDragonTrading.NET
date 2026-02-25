using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.CancelPaymentLink;

/// <summary>
/// Validator for CancelPaymentLinkCommand
/// </summary>
public class CancelPaymentLinkCommandValidator : AbstractValidator<CancelPaymentLinkCommand>
{
    public CancelPaymentLinkCommandValidator()
    {
        RuleFor(x => x.orderCode)
            .GreaterThan(0).WithMessage("Mã đơn hàng phải lớn hơn 0");

        RuleFor(x => x.reason)
            .MaximumLength(500).WithMessage("Lý do hủy không được vượt quá 500 ký tự")
            .When(x => !string.IsNullOrEmpty(x.reason));
    }
}
