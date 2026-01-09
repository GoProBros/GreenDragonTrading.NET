using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Workspace.Queries.GetWorkspaceByShareCode
{
    /// <summary>
    /// Query to get workspace layout by share code
    /// </summary>
    public record GetWorkspaceByShareCodeQuery(
        string ShareCode
    ) : IRequest<ApiResponse<WorkspaceDto>>;
}
