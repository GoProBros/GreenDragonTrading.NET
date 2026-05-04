using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Commands.CreateCategory;

/// <summary>
/// Validator for CreateCategoryCommand
/// </summary>
public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code là bắt buộc")
            .MaximumLength(50).WithMessage("Code không được vượt quá 50 ký tự");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên danh mục là bắt buộc")
            .MaximumLength(200).WithMessage("Tên danh mục không được vượt quá 200 ký tự");

        RuleFor(x => x.Level)
            .InclusiveBetween(1, 4).WithMessage("Level phải từ 1 đến 4");

        RuleFor(x => x.ParentId)
            .MaximumLength(50).WithMessage("Parent ID không được vượt quá 50 ký tự");
    }
}
