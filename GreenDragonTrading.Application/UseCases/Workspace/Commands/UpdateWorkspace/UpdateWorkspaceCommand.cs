using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.UpdateWorkspace
{
    /// <summary>
    /// Command to update an existing workspace layout
    /// </summary>
    public record UpdateWorkspaceCommand(
        Guid WorkspaceId,
        string? WorkspaceName = null,
        string? LayoutJson = null,
        bool? IsDefault = null
    ) : IRequest<ApiResponse<WorkspaceDto>>;
}
