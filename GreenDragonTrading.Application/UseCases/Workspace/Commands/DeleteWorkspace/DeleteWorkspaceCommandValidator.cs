using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.DeleteWorkspace
{
    /// <summary>
    /// Validator for DeleteWorkspaceCommand
    /// </summary>
    public class DeleteWorkspaceCommandValidator : AbstractValidator<DeleteWorkspaceCommand>
    {
        public DeleteWorkspaceCommandValidator()
        {
            RuleFor(x => x.WorkspaceId)
                .GreaterThan(0)
                .WithMessage("WorkspaceId phải lớn hơn 0.");
        }
    }
}
