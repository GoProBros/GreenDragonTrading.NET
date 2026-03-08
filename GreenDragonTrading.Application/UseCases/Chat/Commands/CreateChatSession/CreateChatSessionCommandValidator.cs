using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.CreateChatSession
{
    public class CreateChatSessionCommandValidator : AbstractValidator<CreateChatSessionCommand>
    {
        public CreateChatSessionCommandValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(255)
                .WithMessage("Tiêu đề không được vượt quá 255 ký tự")
                .When(x => !string.IsNullOrWhiteSpace(x.Title));
        }
    }
}
