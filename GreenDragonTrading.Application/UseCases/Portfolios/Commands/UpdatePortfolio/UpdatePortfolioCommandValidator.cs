using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Commands.UpdatePortfolio;

public class UpdatePortfolioCommandValidator : AbstractValidator<UpdatePortfolioCommand>
{
    public UpdatePortfolioCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Portfolio ID phải lớn hơn 0");

        RuleFor(x => x.Ticker)
            .MaximumLength(20).WithMessage("Ticker không được vượt quá 20 ký tự")
            .When(x => x.Ticker != null);

        RuleFor(x => x.Ticker)
            .Must(ticker => !string.IsNullOrWhiteSpace(ticker))
            .WithMessage("Ticker không được chỉ chứa khoảng trắng")
            .When(x => x.Ticker != null);

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Tên portfolio không được vượt quá 100 ký tự")
            .When(x => x.Name != null);

        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Tên portfolio không được chỉ chứa khoảng trắng")
            .When(x => x.Name != null);

        RuleFor(x => x.Status!.Value)
            .IsInEnum().WithMessage("Trạng thái portfolio không hợp lệ")
            .When(x => x.Status.HasValue);
    }
}
