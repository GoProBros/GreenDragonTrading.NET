using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.GetOrCreateDirectSession
{
    public class GetOrCreateDirectSessionCommandValidator : AbstractValidator<GetOrCreateDirectSessionCommand>
    {
        public GetOrCreateDirectSessionCommandValidator()
        {
            RuleFor(x => x.PhoneOrEmail)
                .NotEmpty().WithMessage("Vui lòng nhập số điện thoại hoặc email.")
                .MaximumLength(255).WithMessage("Thông tin tìm kiếm không được vượt quá 255 ký tự.");
        }
    }
}
