using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Commands.DeleteCategory;

/// <summary>
/// Validator for DeleteCategoryCommand
/// </summary>
public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID là bắt buộc")
            .MaximumLength(50).WithMessage("ID không được vượt quá 50 ký tự");
    }
}
