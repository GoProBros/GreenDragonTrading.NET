using FluentValidation;
using GreenDragonTrading.Domain.Constants.DNSE;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportFromDnse;

public class ImportBulkFromDnseCommandValidator : AbstractValidator<ImportBulkFromDnseCommand>
{
    public ImportBulkFromDnseCommandValidator()
    {
        RuleFor(x => x.CycleType)
            .NotEmpty()
            .WithMessage("CycleType không được để trống")
            .Must(ct => ct == DnseConstants.CycleType.QUARTERLY || ct == DnseConstants.CycleType.YEARLY)
            .WithMessage($"CycleType phải là '{DnseConstants.CycleType.QUARTERLY}' hoặc '{DnseConstants.CycleType.YEARLY}'");

        RuleFor(x => x.CycleNumber)
            .Must(cn => cn == 5 || cn == 10)
            .WithMessage("CycleNumber phải là 5 hoặc 10");

        RuleFor(x => x.Tickers)
            .Must(tickers => tickers == null || tickers.Count > 0)
            .WithMessage("Danh sách Tickers không được rỗng nếu được cung cấp");
    }
}
