using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Queries.GetLayoutById;

/// <summary>
/// Validator for GetLayoutByIdQuery
/// </summary>
public class GetLayoutByIdQueryValidator : AbstractValidator<GetLayoutByIdQuery>
{
    public GetLayoutByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Layout ID phải lớn hơn 0");
    }
}
