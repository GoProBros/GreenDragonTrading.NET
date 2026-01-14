using FluentValidation;
using System.Text.Json;

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
                .Must(BeValidJson).WithMessage("Layout JSON không được là undefined");
        }

        private static bool BeValidJson(JsonElement json)
        {
            return json.ValueKind != JsonValueKind.Undefined;
        }
    }
}
