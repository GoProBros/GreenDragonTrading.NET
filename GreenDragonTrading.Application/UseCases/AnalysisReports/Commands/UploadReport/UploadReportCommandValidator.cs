using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Commands.UploadReport;

/// <summary>
/// Validator for UploadReportCommand
/// </summary>
public class UploadReportCommandValidator : AbstractValidator<UploadReportCommand>
{
    public UploadReportCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("File là bắt buộc")
            .Must(file => file.Length > 0).WithMessage("File không được rỗng")
            .Must(file => file.Length <= 50 * 1024 * 1024).WithMessage("File không được vượt quá 50MB");

        RuleFor(x => x.Metadata)
            .NotNull().WithMessage("Metadata là bắt buộc");

        When(x => x.Metadata != null, () =>
        {
            RuleFor(x => x.Metadata.SourceId)
                .NotEmpty().WithMessage("Source ID là bắt buộc")
                .MaximumLength(50).WithMessage("Source ID không được vượt quá 50 ký tự");

            RuleFor(x => x.Metadata.CategoryId)
                .NotEmpty().WithMessage("Category ID là bắt buộc")
                .MaximumLength(50).WithMessage("Category ID không được vượt quá 50 ký tự");

            RuleFor(x => x.Metadata.Title)
                .NotEmpty().WithMessage("Tiêu đề là bắt buộc")
                .MaximumLength(500).WithMessage("Tiêu đề không được vượt quá 500 ký tự");

            RuleFor(x => x.Metadata.Description)
                .MaximumLength(5000).WithMessage("Mô tả không được vượt quá 5000 ký tự");

            RuleFor(x => x.Metadata.Tickers)
                .Must(tickers => tickers == null || tickers.All(t => !string.IsNullOrWhiteSpace(t) && t.Length <= 20))
                .WithMessage("Mỗi ticker phải hợp lệ và không quá 20 ký tự");

            RuleFor(x => x.Metadata.SectorId)
                .MaximumLength(10).WithMessage("Sector ID không được vượt quá 10 ký tự");
        });
    }
}
