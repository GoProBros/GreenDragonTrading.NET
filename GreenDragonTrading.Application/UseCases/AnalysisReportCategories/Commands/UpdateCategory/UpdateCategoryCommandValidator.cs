using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Commands.UpdateCategory;

/// <summary>
/// Validator for UpdateCategoryCommand
/// </summary>
public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID là bắt buộc")
            .MaximumLength(50).WithMessage("ID không được vượt quá 50 ký tự");

        When(x => !string.IsNullOrEmpty(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MaximumLength(200).WithMessage("Tên không được vượt quá 200 ký tự");
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Mô tả không được vượt quá 1000 ký tự");
        });

        When(x => x.Level.HasValue, () =>
        {
            RuleFor(x => x.Level)
                .InclusiveBetween(1, 4).WithMessage("Level phải từ 1 đến 4");
        });

        When(x => x.ParentId != null, () =>
        {
            RuleFor(x => x.ParentId)
                .MaximumLength(50).WithMessage("Parent ID không được vượt quá 50 ký tự");
        });
    }
}
