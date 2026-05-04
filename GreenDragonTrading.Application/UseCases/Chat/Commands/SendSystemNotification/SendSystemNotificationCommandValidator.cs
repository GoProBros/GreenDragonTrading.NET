using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.SendSystemNotification
{
    public class SendSystemNotificationCommandValidator : AbstractValidator<SendSystemNotificationCommand>
    {
        public SendSystemNotificationCommandValidator()
        {
            RuleFor(x => x)
                .Must(x => x.SendToAll || (x.UserIds != null && x.UserIds.Count > 0))
                .WithMessage("UserIds không hợp lệ khi gửi theo danh sách người dùng.");

            RuleFor(x => x)
                .Must(x => !x.SendToAll || x.UserIds == null || x.UserIds.Count == 0)
                .WithMessage("Khi gửi tất cả người dùng, UserIds phải để trống.");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Nội dung thông báo không được để trống.")
                .MaximumLength(4000).WithMessage("Nội dung thông báo không được vượt quá 4000 ký tự.");
        }
    }
}
