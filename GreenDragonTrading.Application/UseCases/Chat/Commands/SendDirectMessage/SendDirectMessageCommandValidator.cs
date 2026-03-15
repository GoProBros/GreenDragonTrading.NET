using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.SendDirectMessage
{
    public class SendDirectMessageCommandValidator : AbstractValidator<SendDirectMessageCommand>
    {
        public SendDirectMessageCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .GreaterThan(0).WithMessage("Session ID không hợp lệ.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Nội dung tin nhắn không được để trống.")
                .MaximumLength(10000).WithMessage("Nội dung tin nhắn không được vượt quá 10000 ký tự.");
        }
    }
}
