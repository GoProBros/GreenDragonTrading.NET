using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Workspace.Queries.GetWorkspaceByShareCode
{
    public class GetWorkspaceByShareCodeQueryValidator : AbstractValidator<GetWorkspaceByShareCodeQuery>
    {
        public GetWorkspaceByShareCodeQueryValidator()
        {
            RuleFor(x => x.ShareCode)
                .NotEmpty().WithMessage("Share code không được để trống")
                .Length(8).WithMessage("Share code phải có 8 ký tự");
        }
    }
}
