using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Common.Utils;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Workspace.Queries.GetWorkspaceByShareCode
{
    public class GetWorkspaceByShareCodeQueryHandler : IRequestHandler<GetWorkspaceByShareCodeQuery, ApiResponse<WorkspaceDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetWorkspaceByShareCodeQueryHandler> _logger;

        public GetWorkspaceByShareCodeQueryHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<GetWorkspaceByShareCodeQueryHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
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

                var allowAllModules = _currentUserService.IsAdminOrStaff || !_currentUserService.IsAuthenticated;
                var allowedModules = new List<string>();
                if (!allowAllModules)
                {
                    var userId = _currentUserService.GetRequiredUserId();
                    var effectiveSubscription = await SubscriptionAccessHelper.GetEffectiveSubscriptionAsync(
                        _uow,
                        false,
                        userId,
                        cancellationToken);

                    allowedModules = WorkspaceLayoutLockingHelper.ParseAllowedModuleKeys(
                        effectiveSubscription?.AllowedModules);
                }

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

                var workspaceDto = new WorkspaceDto
                {
                    Id = workspace.Id,
                    WorkspaceName = workspace.WorkspaceName,
                    LayoutJson = layoutJson,
                    Type = workspace.Type,
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
