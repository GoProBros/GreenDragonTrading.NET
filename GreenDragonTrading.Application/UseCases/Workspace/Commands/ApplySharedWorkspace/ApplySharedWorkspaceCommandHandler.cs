using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.ApplySharedWorkspace
{
    public class ApplySharedWorkspaceCommandHandler : IRequestHandler<ApplySharedWorkspaceCommand, ApiResponse<WorkspaceDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWorkspaceDuplicationService _workspaceDuplicationService;
        private readonly ILogger<ApplySharedWorkspaceCommandHandler> _logger;

        public ApplySharedWorkspaceCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            IWorkspaceDuplicationService workspaceDuplicationService,
            ILogger<ApplySharedWorkspaceCommandHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _workspaceDuplicationService = workspaceDuplicationService;
            _logger = logger;
        }

        public async Task<ApiResponse<WorkspaceDto>> Handle(ApplySharedWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetRequiredUserId();

            var sharedWorkspace = await GetSharedWorkspaceAsync(request.SharedWorkspaceCode, cancellationToken);

            ValidateNotOwnWorkspace(sharedWorkspace, userId);

            if (!_currentUserService.IsAdminOrStaff)
            {
                var activeSubscription = await _uow.UserSubscriptions.GetActiveSubscriptionAsync(userId, cancellationToken);

                if (activeSubscription != null)
                {
                    var existingWorkspaces = await _uow.Workspaces.GetWorkspaceByUserIdAsync(
                        userId,
                        sharedWorkspace.Type,
                        cancellationToken);

                    if (existingWorkspaces.Count >= activeSubscription.Subscription.MaxWorkspaces)
                    {
                        throw new Domain.Exceptions.BusinessRuleException(
                            $"Gói đăng ký của bạn chỉ cho phép tối đa {activeSubscription.Subscription.MaxWorkspaces} workspace cho loại {sharedWorkspace.Type}.");
                    }
                }
            }

            var newWorkspace = await _workspaceDuplicationService.DuplicateWorkspaceAsync(
                sharedWorkspace,
                userId,
                "(Sao chép)",
                cancellationToken);

            var workspaceDto = MapToDto(newWorkspace);

            _logger.LogInformation(
                "Workspace applied successfully from share code {ShareCode}. New workspace ID: {WorkspaceId} for user: {UserId}",
                request.SharedWorkspaceCode,
                newWorkspace.Id,
                userId);

            return ApiResponse<WorkspaceDto>.Success(workspaceDto, "Áp dụng workspace thành công.");
        }

        private async Task<Domain.Entities.Workspace> GetSharedWorkspaceAsync(string shareCode, CancellationToken cancellationToken)
        {
            var sharedWorkspace = await _uow.Workspaces.GetByShareCodeAsync(shareCode, cancellationToken);

            if (sharedWorkspace == null)
            {
                _logger.LogWarning("Shared workspace not found for code: {ShareCode}", shareCode);
                throw new NotFoundException("Workspace chia sẻ không tồn tại.");
            }

            return sharedWorkspace;
        }

        private void ValidateNotOwnWorkspace(Domain.Entities.Workspace sharedWorkspace, Guid userId)
        {
            if (sharedWorkspace.UserId == userId)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to apply their own workspace share code: {ShareCode}",
                    userId,
                    sharedWorkspace.ShareCode);
                throw new BusinessRuleException("Bạn không thể áp dụng mã chia sẻ của chính mình.");
            }
        }

        private static WorkspaceDto MapToDto(Domain.Entities.Workspace workspace)
        {
            return new WorkspaceDto
            {
                Id = workspace.Id,
                WorkspaceName = workspace.WorkspaceName,
                LayoutJson = JsonSerializer.Deserialize<JsonElement>(workspace.LayoutJson),
                Type = workspace.Type,
                IsDefault = workspace.IsDefault,
                ShareCode = workspace.ShareCode
            };
        }
    }
}