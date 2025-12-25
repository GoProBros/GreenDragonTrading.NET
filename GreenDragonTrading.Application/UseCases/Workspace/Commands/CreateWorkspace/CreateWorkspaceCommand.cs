using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.CreateWorkspace
{
    /// <summary>
    /// Command to create a new workspace layout
    /// </summary>
    public record CreateWorkspaceCommand(
        string WorkspaceName,
        string LayoutJson,
        bool IsDefault = false
    ) : IRequest<ApiResponse<WorkspaceDto>>;
}
