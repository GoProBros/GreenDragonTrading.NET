using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Files.Queries.DownloadFile;

/// <summary>
/// Validator for DownloadFileQuery
/// </summary>
public class DownloadFileQueryValidator : AbstractValidator<DownloadFileQuery>
{
    public DownloadFileQueryValidator()
    {
        RuleFor(x => x.Category)
            .IsInEnum()
            .WithMessage("Loại file không hợp lệ");

        RuleFor(x => x.EntityId)
            .NotEmpty()
            .WithMessage("ID entity là bắt buộc");
    }
}
