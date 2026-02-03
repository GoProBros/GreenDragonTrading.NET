using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Commands.UpdateReport;

/// <summary>
/// Validator for UpdateReportCommand
/// </summary>
public class UpdateReportCommandValidator : AbstractValidator<UpdateReportCommand>
{
    public UpdateReportCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID là bắt buộc");

        When(x => !string.IsNullOrEmpty(x.SourceId), () =>
        {
            RuleFor(x => x.SourceId)
                .MaximumLength(50).WithMessage("Source ID không được vượt quá 50 ký tự");
        });

        When(x => !string.IsNullOrEmpty(x.CategoryId), () =>
        {
            RuleFor(x => x.CategoryId)
                .MaximumLength(50).WithMessage("Category ID không được vượt quá 50 ký tự");
        });

        When(x => !string.IsNullOrEmpty(x.Title), () =>
        {
            RuleFor(x => x.Title)
                .MaximumLength(500).WithMessage("Tiêu đề không được vượt quá 500 ký tự");
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(5000).WithMessage("Mô tả không được vượt quá 5000 ký tự");
        });

        When(x => x.Tickers != null, () =>
        {
            RuleFor(x => x.Tickers)
                .Must(tickers => tickers == null || tickers.All(t => !string.IsNullOrWhiteSpace(t) && t.Length <= 20))
                .WithMessage("Mỗi ticker phải hợp lệ và không quá 20 ký tự");
        });

        When(x => x.SectorId != null, () =>
        {
            RuleFor(x => x.SectorId)
                .MaximumLength(10).WithMessage("Sector ID không được vượt quá 10 ký tự");
        });
    }
}
