using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.UploadFinancialReport
{
    public class UploadFinancialReportCommandValidator : AbstractValidator<UploadFinancialReportCommand>
    {
        private static readonly string[] AllowedExtensions = { ".pdf", ".xlsx", ".xls", ".docx", ".doc" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public UploadFinancialReportCommandValidator()
        {
            RuleFor(x => x.Ticker)
                .NotEmpty().WithMessage("Ticker không được để trống")
                .MaximumLength(20).WithMessage("Ticker không được vượt quá 20 ký tự");

            RuleFor(x => x.Year)
                .GreaterThan(2000).WithMessage("Năm phải lớn hơn 2000")
                .LessThanOrEqualTo(DateTime.Now.Year).WithMessage($"Năm không được vượt quá {DateTime.Now.Year}");

            RuleFor(x => x.File)
                .NotNull().WithMessage("File không được để trống")
                .Must(file => file != null && file.Length > 0)
                .WithMessage("File không hợp lệ hoặc rỗng")
                .Must(file => file == null || file.Length <= MaxFileSize)
                .WithMessage($"Kích thước file không được vượt quá {MaxFileSize / 1024 / 1024}MB")
                .Must(file => file == null || AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                .WithMessage($"Chỉ chấp nhận file: {string.Join(", ", AllowedExtensions)}");
        }
    }
}
