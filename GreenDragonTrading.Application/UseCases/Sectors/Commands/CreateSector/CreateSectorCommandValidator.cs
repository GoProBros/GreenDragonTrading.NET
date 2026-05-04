using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Sectors.Commands.CreateSector
{
    public class CreateSectorCommandValidator : AbstractValidator<CreateSectorCommand>
    {
        public CreateSectorCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Sector ID is required")
                .MaximumLength(10).WithMessage("Sector ID must not exceed 10 characters");

            RuleFor(x => x.EnName)
                .MaximumLength(100).WithMessage("English name must not exceed 100 characters");

            RuleFor(x => x.ViName)
                .MaximumLength(100).WithMessage("Vietnamese name must not exceed 100 characters");

            RuleFor(x => x.Level)
                .InclusiveBetween(1, 4).WithMessage("Level must be between 1 and 4");

            RuleFor(x => x.ParentId)
                .MaximumLength(10).WithMessage("Parent ID must not exceed 10 characters")
                .Must((cmd, parentId) => cmd.Level == 1 ? string.IsNullOrEmpty(parentId) : !string.IsNullOrEmpty(parentId))
                .WithMessage("Level 1 sectors must not have a parent. Levels 2-4 must have a parent.");
        }
    }
}
