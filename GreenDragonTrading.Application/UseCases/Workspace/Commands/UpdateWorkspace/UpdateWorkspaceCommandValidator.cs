using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.UpdateWorkspace
{
    public class UpdateWorkspaceCommandValidator : AbstractValidator<UpdateWorkspaceCommand>
    {
        public UpdateWorkspaceCommandValidator()
        {
            RuleFor(x => x.WorkspaceId)
                .NotEmpty().WithMessage("Workspace ID không được để trống");

            When(x => !string.IsNullOrEmpty(x.WorkspaceName), () =>
            {
                RuleFor(x => x.WorkspaceName)
                    .MaximumLength(100).WithMessage("Tên workspace không được vượt quá 100 ký tự");
            });
        }
    }
}
