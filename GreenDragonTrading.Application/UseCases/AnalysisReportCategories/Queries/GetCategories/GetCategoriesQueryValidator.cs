using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Queries.GetCategories;

/// <summary>
/// Validator for GetCategoriesQuery
/// </summary>
public class GetCategoriesQueryValidator : AbstractValidator<GetCategoriesQuery>
{
    public GetCategoriesQueryValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThan(0).WithMessage("PageIndex phải lớn hơn 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize phải lớn hơn 0")
            .LessThanOrEqualTo(100).WithMessage("PageSize không được vượt quá 100");

        When(x => x.Level.HasValue, () =>
        {
            RuleFor(x => x.Level)
                .InclusiveBetween(1, 4).WithMessage("Level phải từ 1 đến 4");
        });
    }
}
