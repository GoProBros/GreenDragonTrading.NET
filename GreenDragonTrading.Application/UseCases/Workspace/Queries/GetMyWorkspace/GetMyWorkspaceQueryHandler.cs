using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Workspace.Queries.GetMyWorkspace
{
    /// <summary>
    /// Handler for GetMyWorkspaceQuery
    /// </summary>
    public class GetMyWorkspaceQueryHandler : IRequestHandler<GetMyWorkspaceQuery, ApiResponse<List<WorkspaceDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetMyWorkspaceQueryHandler> _logger;

        public GetMyWorkspaceQueryHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<GetMyWorkspaceQueryHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<List<WorkspaceDto>>> Handle(GetMyWorkspaceQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetRequiredUserId();

            var workspaces = await _uow.Workspaces.GetWorkspaceByUserIdAsync(userId, cancellationToken);
            var result = workspaces.Select(w =>
            {
                JsonElement? layoutJson = null;
                if (!string.IsNullOrEmpty(w.LayoutJson))
                {
                    using var doc = JsonDocument.Parse(w.LayoutJson);
                    layoutJson = doc.RootElement.Clone();
                }
                return new WorkspaceDto
                {
                    Id = w.Id,
                    WorkspaceName = w.WorkspaceName,
                    LayoutJson = layoutJson,
                    IsDefault = w.IsDefault,
                    ShareCode = w.ShareCode
                };
            }).ToList();

            _logger.LogInformation("User workspaces retrieved successfully: {UserId}", userId);
            return ApiResponse<List<WorkspaceDto>>.Success(result, "Lấy thành công danh sách workspace của người dùng.");
        }
    }
}
