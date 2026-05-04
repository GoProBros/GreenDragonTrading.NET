using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Payments.Queries.GetPaymentStatus;

/// <summary>
/// Validator for GetPaymentStatusQuery
/// </summary>
public class GetPaymentStatusQueryValidator : AbstractValidator<GetPaymentStatusQuery>
{
    public GetPaymentStatusQueryValidator()
    {
        RuleFor(x => x.OrderCode)
            .GreaterThan(0).WithMessage("Mã đơn hàng phải lớn hơn 0");
    }
}
