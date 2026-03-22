using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Users.Queries.GetUsers;

/// <summary>
/// Validator for GetUsersQuery.
/// </summary>
public class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThan(0)
            .WithMessage("PageIndex phải lớn hơn 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize phải trong khoảng từ 1 đến 100.");
    }
}