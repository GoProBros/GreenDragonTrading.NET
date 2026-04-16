using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Commands.CreatePortfolio;

public class CreatePortfolioCommandValidator : AbstractValidator<CreatePortfolioCommand>
{
    public CreatePortfolioCommandValidator()
    {
        RuleFor(x => x.Ticker)
            .NotEmpty().WithMessage("Ticker không được để trống")
            .MaximumLength(20).WithMessage("Ticker không được vượt quá 20 ký tự");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Tên portfolio không được vượt quá 100 ký tự")
            .When(x => x.Name != null);

        RuleFor(x => x.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Tên portfolio không được chỉ chứa khoảng trắng")
            .When(x => x.Name != null);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Trạng thái portfolio không hợp lệ");
    }
}
