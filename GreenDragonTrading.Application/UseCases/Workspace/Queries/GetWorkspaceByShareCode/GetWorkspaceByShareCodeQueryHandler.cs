using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Workspace.Queries.GetWorkspaceByShareCode
{
    public class GetWorkspaceByShareCodeQueryHandler : IRequestHandler<GetWorkspaceByShareCodeQuery, ApiResponse<WorkspaceDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<GetWorkspaceByShareCodeQueryHandler> _logger;

        public GetWorkspaceByShareCodeQueryHandler(
            IUnitOfWork uow,
            ILogger<GetWorkspaceByShareCodeQueryHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<ApiResponse<WorkspaceDto>> Handle(GetWorkspaceByShareCodeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var workspace = await _uow.Workspaces.GetByShareCodeAsync(request.ShareCode, cancellationToken);

                if (workspace == null)
                {
                    throw new NotFoundException($"Không tìm thấy workspace với share code: {request.ShareCode}");
                }

                var workspaceDto = new WorkspaceDto
                {
                    Id = workspace.Id,
                    WorkspaceName = workspace.WorkspaceName,
                    LayoutJson = workspace.LayoutJson,
                    IsDefault = workspace.IsDefault,
                    ShareCode = workspace.ShareCode
                };

                _logger.LogInformation("Workspace retrieved successfully with share code: {ShareCode}", request.ShareCode);
                return ApiResponse<WorkspaceDto>.Success(workspaceDto, "Lấy workspace thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workspace with share code: {ShareCode}", request.ShareCode);
                throw;
            }
        }
    }
}
