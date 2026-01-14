using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.UploadFile
{
    public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
    {
        public UploadFileCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID không được để trống.");

            RuleFor(x => x.File)
                .NotNull().WithMessage("File không được để trống.");

            When(x => x.File != null, () =>
            {
                RuleFor(x => x.File.Length)
                    .GreaterThan(0).WithMessage("File không được rỗng.")
                    .LessThanOrEqualTo(50 * 1024 * 1024).WithMessage("File không được vượt quá 50MB.");

                RuleFor(x => x.File.FileName)
                    .Must(fileName => 
                    {
                        var extension = Path.GetExtension(fileName).ToLowerInvariant();
                        return new[] { ".pdf", ".xlsx", ".xls", ".doc", ".docx" }.Contains(extension);
                    })
                    .WithMessage("File chỉ được phép là PDF, Excel hoặc Word.");
            });
        }
    }
}
