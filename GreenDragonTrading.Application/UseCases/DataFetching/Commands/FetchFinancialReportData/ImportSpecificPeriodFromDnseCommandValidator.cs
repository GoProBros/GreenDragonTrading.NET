using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSpecificPeriodFromDnse;

public class ImportSpecificPeriodFromDnseCommandValidator : AbstractValidator<ImportSpecificPeriodFromDnseCommand>
{
    public ImportSpecificPeriodFromDnseCommandValidator()
    {
        RuleFor(x => x.Ticker)
            .NotEmpty()
            .WithMessage("Ticker không được để trống")
            .MaximumLength(20)
            .WithMessage("Ticker không được vượt quá 20 ký tự");

        RuleFor(x => x.Year)
            .GreaterThan(2000)
            .WithMessage("Năm phải lớn hơn 2000")
            .LessThanOrEqualTo(DateTime.Now.Year + 1)
            .WithMessage("Năm không được lớn hơn năm hiện tại + 1");

        When(x => x.Quarter.HasValue, () =>
        {
            RuleFor(x => x.Quarter!.Value)
                .IsInEnum().WithMessage("Kỳ báo cáo không hợp lệ.");
        });
    }
}
