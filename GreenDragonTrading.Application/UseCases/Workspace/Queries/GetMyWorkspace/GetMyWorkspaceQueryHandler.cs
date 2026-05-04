using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Common.Utils;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Workspace.Queries.GetMyWorkspace
{
    /// <summary>
    /// Handler for GetMyWorkspaceQuery
    /// </summary>
    public class GetMyWorkspaceQueryHandler : IRequestHandler<GetMyWorkspaceQuery, ApiResponse<MyWorkspacesDto>>
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

        public async Task<ApiResponse<MyWorkspacesDto>> Handle(GetMyWorkspaceQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetRequiredUserId();

            var allowAllModules = _currentUserService.IsAdminOrStaff;
            var allowedModules = new List<string>();
            if (!allowAllModules)
            {
                var activeSubscription = await _uow.UserSubscriptions.GetActiveSubscriptionAsync(userId, cancellationToken);
                allowedModules = WorkspaceLayoutLockingHelper.ParseAllowedModuleKeys(activeSubscription?.Subscription?.AllowedModules);
            }

            _logger.LogInformation("Workspaces retrieved successfully for user: {UserId}", userId);

            var webWorkspaces = await GetWorkspacesByTypeAsync(
                userId,
                WorkspaceType.Web,
                allowedModules,
                allowAllModules,
                cancellationToken);

            var mobileWorkspaces = await GetWorkspacesByTypeAsync(
                userId,
                WorkspaceType.Mobile,
                allowedModules,
                allowAllModules,
                cancellationToken);

            var result = new MyWorkspacesDto
            {
                WebWorkspaces = webWorkspaces,
                MobileWorkspaces = mobileWorkspaces
            };

            return ApiResponse<MyWorkspacesDto>.Success(result, "Lấy thành công danh sách workspace của người dùng.");
        }

        private async Task<List<WorkspaceDto>> GetWorkspacesByTypeAsync(
            Guid userId,
            WorkspaceType type,
            IReadOnlyCollection<string> allowedModules,
            bool allowAllModules,
            CancellationToken cancellationToken)
        {
            List<Domain.Entities.Workspace> workspaces;

            if (_currentUserService.IsAdminOrStaff)
            {
                var ownWorkspaces = await _uow.Workspaces.GetWorkspaceByUserIdAsync(userId, type, cancellationToken);
                var systemWorkspaces = await _uow.Workspaces.GetSystemWorkspacesAsync(type, cancellationToken);
                workspaces = ownWorkspaces.Concat(systemWorkspaces).ToList();
            }
            else
            {
                workspaces = await _uow.Workspaces.GetWorkspaceByUserIdAsync(userId, type, cancellationToken);
            }

            return workspaces.Select(w => MapToDto(w, allowedModules, allowAllModules)).ToList();
        }

        private static WorkspaceDto MapToDto(
            Domain.Entities.Workspace workspace,
            IReadOnlyCollection<string> allowedModules,
            bool allowAllModules)
        {
            JsonElement? layoutJson = null;
            if (!string.IsNullOrEmpty(workspace.LayoutJson))
            {
                using var doc = JsonDocument.Parse(workspace.LayoutJson);
                layoutJson = doc.RootElement.Clone();
            }

            layoutJson = WorkspaceLayoutLockingHelper.ApplyModuleLocks(
                layoutJson,
                allowedModules,
                allowAllModules);

            return new WorkspaceDto
            {
                Id = workspace.Id,
                WorkspaceName = workspace.WorkspaceName,
                LayoutJson = layoutJson,
                Type = workspace.Type,
                IsDefault = workspace.IsDefault,
                ShareCode = workspace.ShareCode
            };
        }
    }
}
