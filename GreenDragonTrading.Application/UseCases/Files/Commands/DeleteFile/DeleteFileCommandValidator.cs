using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Files.Commands.DeleteFile;

/// <summary>
/// Validator for DeleteFileCommand
/// </summary>
public class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.Category)
            .IsInEnum()
            .WithMessage("Loại file không hợp lệ");

        RuleFor(x => x.EntityId)
            .NotEmpty()
            .WithMessage("ID entity là bắt buộc");
    }
}
