using FluentValidation;
using GreenDragonTrading.Domain.Constants;

namespace GreenDragonTrading.Application.UseCases.Files.Commands.UploadFile;

/// <summary>
/// Validator for upload file command
/// </summary>
public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(FileConstants.MAX_FILE_SIZE)
            .WithMessage($"File size must not exceed {FileConstants.MAX_FILE_SIZE / 1024 / 1024}MB")
            .When(x => x.File != null);

        RuleFor(x => x.File.FileName)
            .Must(fileName =>
            {
                var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
                return extension != null && FileConstants.AllowedExtensions.All.Contains(extension);
            })
            .WithMessage($"Only {string.Join(", ", FileConstants.AllowedExtensions.All)} files are allowed")
            .When(x => x.File != null);

        RuleFor(x => x.Metadata)
            .NotNull()
            .WithMessage("Metadata is required");

        RuleFor(x => x.Metadata.Category)
            .IsInEnum()
            .WithMessage("Invalid file category")
            .When(x => x.Metadata != null);
    }
}
