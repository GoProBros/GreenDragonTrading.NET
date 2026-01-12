using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.UpdateWorkspace
{
    /// <summary>
    /// Command to update an existing workspace layout
    /// </summary>
    public record UpdateWorkspaceCommand(
        int WorkspaceId,
        string? WorkspaceName = null,
        JsonElement? LayoutJson = null,
        bool? IsDefault = null
    ) : IRequest<ApiResponse<WorkspaceDto>>;
}
