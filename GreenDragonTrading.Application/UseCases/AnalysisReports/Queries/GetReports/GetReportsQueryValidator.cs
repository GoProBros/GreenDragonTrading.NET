using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Queries.GetReports;

/// <summary>
/// Validator for GetReportsQuery
/// </summary>
public class GetReportsQueryValidator : AbstractValidator<GetReportsQuery>
{
    public GetReportsQueryValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThan(0).WithMessage("PageIndex phải lớn hơn 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize phải lớn hơn 0")
            .LessThanOrEqualTo(100).WithMessage("PageSize không được vượt quá 100");

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

        When(x => !string.IsNullOrEmpty(x.Ticker), () =>
        {
            RuleFor(x => x.Ticker)
                .MaximumLength(20).WithMessage("Ticker không được vượt quá 20 ký tự");
        });

        When(x => !string.IsNullOrEmpty(x.SectorId), () =>
        {
            RuleFor(x => x.SectorId)
                .MaximumLength(10).WithMessage("Sector ID không được vượt quá 10 ký tự");
        });

        When(x => !string.IsNullOrEmpty(x.SearchTerm), () =>
        {
            RuleFor(x => x.SearchTerm)
                .MaximumLength(200).WithMessage("Search term không được vượt quá 200 ký tự");
        });
    }
}
