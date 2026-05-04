using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.CreateWorkspace
{
    /// <summary>
    /// Command to create a new workspace layout
    /// </summary>
    public record CreateWorkspaceCommand(
        string WorkspaceName,
        JsonElement LayoutJson,
        WorkspaceType Type,
        bool IsDefault = false
    ) : IRequest<ApiResponse<WorkspaceDto>>;
}
