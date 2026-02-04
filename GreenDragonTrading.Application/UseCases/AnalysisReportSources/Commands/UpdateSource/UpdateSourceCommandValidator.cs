using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportSources.Commands.UpdateSource;

/// <summary>
/// Validator for UpdateSourceCommand
/// </summary>
public class UpdateSourceCommandValidator : AbstractValidator<UpdateSourceCommand>
{
    public UpdateSourceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID là bắt buộc");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên nguồn là bắt buộc")
            .MaximumLength(200).WithMessage("Tên nguồn không được vượt quá 200 ký tự");

        RuleFor(x => x.Website)
            .MaximumLength(500).WithMessage("Website không được vượt quá 500 ký tự")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Website không hợp lệ");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("Logo URL không được vượt quá 500 ký tự");
    }
}
