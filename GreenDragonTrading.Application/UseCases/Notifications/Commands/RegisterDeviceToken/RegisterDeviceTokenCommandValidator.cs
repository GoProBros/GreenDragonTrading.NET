using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Notifications.Commands.RegisterDeviceToken
{
    public class RegisterDeviceTokenCommandValidator : AbstractValidator<RegisterDeviceTokenCommand>
    {
        public RegisterDeviceTokenCommandValidator()
        {
            RuleFor(x => x.ExpoPushToken)
                .NotEmpty().WithMessage("FCM push token không được để trống.")
                .MinimumLength(10).WithMessage("FCM push token không hợp lệ.");
        }
    }
}
