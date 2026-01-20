using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Symbols.Commands.UpdateSymbolSector
{
    public class UpdateSymbolSectorCommandValidator : AbstractValidator<UpdateSymbolSectorCommand>
    {
        public UpdateSymbolSectorCommandValidator()
        {
            RuleFor(x => x.Ticker)
                .NotEmpty().WithMessage("Ticker is required")
                .MaximumLength(20).WithMessage("Ticker must not exceed 20 characters");

            RuleFor(x => x.SectorId)
                .MaximumLength(10).WithMessage("Sector ID must not exceed 10 characters");
        }
    }
}
