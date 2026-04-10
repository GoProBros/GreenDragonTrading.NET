using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.MacroeconomicData.Commands.UpsertMacroeconomicData
{
    public class UpsertMacroeconomicDataCommandValidator : AbstractValidator<UpsertMacroeconomicDataCommand>
    {
        public UpsertMacroeconomicDataCommandValidator()
        {
            RuleFor(x => x.RecordDate)
                .NotEmpty().WithMessage("Ngày ghi nhận không được để trống.");
        }
    }
}