using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Notifications.Commands.RegisterDeviceToken
{
    public class RegisterDeviceTokenCommandValidator : AbstractValidator<RegisterDeviceTokenCommand>
    {
        public RegisterDeviceTokenCommandValidator()
        {
            RuleFor(x => x.ExpoPushToken)
                .NotEmpty().WithMessage("Expo push token không được để trống.")
                .Must(t => t.StartsWith("ExponentPushToken[") && t.EndsWith("]"))
                .WithMessage("Expo push token không hợp lệ. Phải có dạng ExponentPushToken[...].");
        }
    }
}
