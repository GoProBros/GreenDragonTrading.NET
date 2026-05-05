using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.UpdateProactiveAlertLayerBSettings
{
    public class UpdateProactiveAlertLayerBSettingsCommandValidator
        : AbstractValidator<UpdateProactiveAlertLayerBSettingsCommand>
    {
        public UpdateProactiveAlertLayerBSettingsCommandValidator()
        {
            RuleFor(x => x.MinAbsoluteMovePercent)
                .GreaterThanOrEqualTo(0).WithMessage("MinAbsoluteMovePercent must be >= 0.");

            RuleFor(x => x.AtrMoveMultiplier)
                .GreaterThan(0).WithMessage("AtrMoveMultiplier must be > 0.");

            RuleFor(x => x.MinVolumeRatio)
                .GreaterThan(0).WithMessage("MinVolumeRatio must be > 0.");

            RuleFor(x => x.MinAdx)
                .GreaterThan(0).WithMessage("MinAdx must be > 0.");

            RuleFor(x => x.MaxIndicatorSnapshotAgeMinutes)
                .GreaterThan(0).WithMessage("MaxIndicatorSnapshotAgeMinutes must be > 0.");
        }
    }
}
