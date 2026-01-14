using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.ApplySharedWorkspace
{
    public record ApplySharedWorkspaceCommand(
        string SharedWorkspaceCode
        ) : IRequest<ApiResponse<WorkspaceDto>>;

}
