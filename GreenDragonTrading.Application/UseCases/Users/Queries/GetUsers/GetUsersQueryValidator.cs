using FluentValidation;
using GreenDragonTrading.Domain.Enums;

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

        RuleFor(x => x.Role!.Value)
            .IsInEnum()
            .When(x => x.Role.HasValue)
            .WithMessage("Role không hợp lệ.");

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithMessage("Từ khóa tìm kiếm tối đa 100 ký tự.");
    }
}