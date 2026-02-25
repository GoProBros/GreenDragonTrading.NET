using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Queries.GetMyLayouts;

/// <summary>
/// Validator for GetMyLayoutsQuery
/// </summary>
public class GetMyLayoutsQueryValidator : AbstractValidator<GetMyLayoutsQuery>
{
    public GetMyLayoutsQueryValidator()
    {
        RuleFor(x => x.ModuleType)
            .IsInEnum().WithMessage("Loại module không hợp lệ");
    }
}
