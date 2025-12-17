using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Workspace.Queries.GetMyWorkspace
{
    public record GetMyWorkspaceQuery : IRequest<ApiResponse<List<WorkspaceDto>>>;

}
