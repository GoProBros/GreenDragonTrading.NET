using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.SendChatMessage
{
    public class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
    {
        public SendChatMessageCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .GreaterThan(0)
                .WithMessage("Session ID phải lớn hơn 0");

            RuleFor(x => x.Message)
                .NotEmpty()
                .WithMessage("Nội dung tin nhắn không được để trống")
                .MaximumLength(10000)
                .WithMessage("Nội dung tin nhắn không được vượt quá 10,000 ký tự");
        }
    }
}
