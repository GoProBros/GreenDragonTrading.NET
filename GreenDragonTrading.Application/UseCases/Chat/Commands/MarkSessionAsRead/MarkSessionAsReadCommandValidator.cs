using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.MarkSessionAsRead;

public class MarkSessionAsReadCommandValidator : AbstractValidator<MarkSessionAsReadCommand>
{
    public MarkSessionAsReadCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .GreaterThan(0)
            .WithMessage("Session ID phải lớn hơn 0");
    }
}
