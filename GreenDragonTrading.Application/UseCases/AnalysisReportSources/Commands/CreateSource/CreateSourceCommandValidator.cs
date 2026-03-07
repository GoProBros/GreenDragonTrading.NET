using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportSources.Commands.CreateSource;

/// <summary>
/// Validator for CreateSourceCommand
/// </summary>
public class CreateSourceCommandValidator : AbstractValidator<CreateSourceCommand>
{
    public CreateSourceCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code là bắt buộc")
            .MaximumLength(50).WithMessage("Code không được vượt quá 50 ký tự")
            .Matches(@"^[a-zA-Z0-9-_]+$").WithMessage("Code chỉ được chứa chữ thường, số, dấu gạch ngang và gạch dưới");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên nguồn là bắt buộc")
            .MaximumLength(200).WithMessage("Tên nguồn không được vượt quá 200 ký tự");

        RuleFor(x => x.Website)
            .MaximumLength(500).WithMessage("Website không được vượt quá 500 ký tự")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out _))
            .WithMessage("Website không hợp lệ");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("Logo URL không được vượt quá 500 ký tự");
    }
}
