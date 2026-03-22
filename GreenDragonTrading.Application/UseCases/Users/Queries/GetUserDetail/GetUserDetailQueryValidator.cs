using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Users.Queries.GetUserDetail;

/// <summary>
/// Validator for GetUserDetailQuery.
/// </summary>
public class GetUserDetailQueryValidator : AbstractValidator<GetUserDetailQuery>
{
    public GetUserDetailQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId không hợp lệ.");
    }
}