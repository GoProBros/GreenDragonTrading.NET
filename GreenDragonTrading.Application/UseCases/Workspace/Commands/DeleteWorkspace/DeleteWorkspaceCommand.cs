using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.DeleteWorkspace
{
    /// <summary>
    /// Command to delete a workspace by ID
    /// </summary>
    public record DeleteWorkspaceCommand(int WorkspaceId) : IRequest<ApiResponse>;
}
