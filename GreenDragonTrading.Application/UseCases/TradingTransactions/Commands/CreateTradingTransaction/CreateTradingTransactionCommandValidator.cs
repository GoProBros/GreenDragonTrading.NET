using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.TradingTransactions.Commands.CreateTradingTransaction;

public class CreateTradingTransactionCommandValidator : AbstractValidator<CreateTradingTransactionCommand>
{
    public CreateTradingTransactionCommandValidator()
    {
        RuleFor(x => x.PortfolioId)
            .GreaterThan(0).WithMessage("Portfolio ID phải lớn hơn 0");

        RuleFor(x => x.Ticker)
            .NotEmpty().WithMessage("Ticker không được để trống")
            .MaximumLength(20).WithMessage("Ticker không được vượt quá 20 ký tự");

        RuleFor(x => x.Side)
            .NotNull().WithMessage("Loại giao dịch là bắt buộc")
            .IsInEnum().WithMessage("Loại giao dịch không hợp lệ");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0")
            .When(x => x.Quantity.HasValue);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Giá phải lớn hơn hoặc bằng 0")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Fee)
            .GreaterThanOrEqualTo(0).WithMessage("Phí phải lớn hơn hoặc bằng 0")
            .When(x => x.Fee.HasValue);

        RuleFor(x => x.Tax)
            .GreaterThanOrEqualTo(0).WithMessage("Thuế phải lớn hơn hoặc bằng 0")
            .When(x => x.Tax.HasValue);
    }
}
