using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.ApplySharedWorkspace
{
    public class ApplySharedWorkspaceCommandValidator : AbstractValidator<ApplySharedWorkspaceCommand>
    {
        public ApplySharedWorkspaceCommandValidator()
        {
            RuleFor(x => x.SharedWorkspaceCode)
                .NotEmpty()
                .WithMessage("Mã chia sẻ workspace không được để trống.")
                .Length(8)
                .WithMessage("Mã chia sẻ workspace phải có 8 ký tự.")
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage("Mã chia sẻ workspace chỉ chứa chữ cái và số.");
        }
    }
}