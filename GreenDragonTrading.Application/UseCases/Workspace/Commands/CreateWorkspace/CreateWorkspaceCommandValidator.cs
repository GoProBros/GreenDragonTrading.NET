using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.CreateWorkspace
{
    public class CreateWorkspaceCommandValidator : AbstractValidator<CreateWorkspaceCommand>
    {
        public CreateWorkspaceCommandValidator()
        {
            RuleFor(x => x.WorkspaceName)
                .NotEmpty().WithMessage("Tên workspace không được để trống")
                .MaximumLength(100).WithMessage("Tên workspace không được vượt quá 100 ký tự");

            RuleFor(x => x.LayoutJson)
                .NotEmpty().WithMessage("Layout JSON không được để trống")
                .Must(BeValidJson).WithMessage("Layout JSON không hợp lệ");
        }

        private static bool BeValidJson(string json)
        {
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
