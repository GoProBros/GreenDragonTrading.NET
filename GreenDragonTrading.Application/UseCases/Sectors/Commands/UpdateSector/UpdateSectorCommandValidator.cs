using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Sectors.Commands.UpdateSector
{
    public class UpdateSectorCommandValidator : AbstractValidator<UpdateSectorCommand>
    {
        public UpdateSectorCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Sector ID is required")
                .MaximumLength(10).WithMessage("Sector ID must not exceed 10 characters");

            RuleFor(x => x.EnName)
                .MaximumLength(100).WithMessage("English name must not exceed 100 characters");

            RuleFor(x => x.ViName)
                .MaximumLength(100).WithMessage("Vietnamese name must not exceed 100 characters");
        }
    }
}
