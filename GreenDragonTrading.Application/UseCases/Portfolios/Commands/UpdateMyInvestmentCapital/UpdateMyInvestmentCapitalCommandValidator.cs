using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Commands.UpdateMyInvestmentCapital;

public class UpdateMyInvestmentCapitalCommandValidator : AbstractValidator<UpdateMyInvestmentCapitalCommand>
{
    public UpdateMyInvestmentCapitalCommandValidator()
    {
        RuleFor(x => x.InvestmentCapital)
            .GreaterThanOrEqualTo(0m)
            .WithMessage("Số tiền đầu tư khả dụng phải lớn hơn hoặc bằng 0");
    }
}