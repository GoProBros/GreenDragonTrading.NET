using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.SendSystemNotification
{
    public class SendSystemNotificationCommandValidator : AbstractValidator<SendSystemNotificationCommand>
    {
        public SendSystemNotificationCommandValidator()
        {
            RuleFor(x => x)
                .Must(x => x.SendToAll || x.UserId.HasValue)
                .WithMessage("UserId không hợp lệ khi gửi cho một người.");

            RuleFor(x => x)
                .Must(x => !x.SendToAll || !x.UserId.HasValue)
                .WithMessage("Khi gửi tất cả người dùng, UserId phải để trống.");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Nội dung thông báo không được để trống.")
                .MaximumLength(4000).WithMessage("Nội dung thông báo không được vượt quá 4000 ký tự.");
        }
    }
}
