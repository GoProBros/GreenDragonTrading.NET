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

            When(x => !string.IsNullOrEmpty(x.LayoutJson), () =>
            {
                RuleFor(x => x.LayoutJson)
                    .Must(BeValidJson).WithMessage("Layout JSON không hợp lệ");
            });
        }

        private static bool BeValidJson(string? json)
        {
            if (string.IsNullOrEmpty(json)) return true;
            
            try
            {
                System.Text.Json.JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
