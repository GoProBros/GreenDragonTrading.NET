using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.UpdateWorkspace
{
    /// <summary>
    /// Handler for UpdateWorkspaceCommand
    /// </summary>
    public class UpdateWorkspaceCommandHandler : IRequestHandler<UpdateWorkspaceCommand, ApiResponse<WorkspaceDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateWorkspaceCommandHandler> _logger;

        public UpdateWorkspaceCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<UpdateWorkspaceCommandHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<WorkspaceDto>> Handle(UpdateWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var workspace = await _uow.Workspaces.GetByIdAsync(request.WorkspaceId, cancellationToken);
            if (workspace == null)
            {
                throw new NotFoundException("Không tìm thấy workspace.");
            }

            var userId = _currentUserService.GetRequiredUserId();
            var isAdminOrStaff = _currentUserService.IsAdminOrStaff;
            var isOwner = workspace.UserId == userId;
            var canAdminOrStaffUpdate = isAdminOrStaff && workspace.IsDefault;
            if (!isOwner && !canAdminOrStaffUpdate)
            {
                throw new AccessDeniedException("Bạn không có quyền cập nhật workspace này.");
            }

            if (!string.IsNullOrEmpty(request.WorkspaceName))
            {
                workspace.WorkspaceName = request.WorkspaceName;
            }

            if (request.LayoutJson.HasValue)
            {
                workspace.LayoutJson = JsonSerializer.Serialize(request.LayoutJson.Value);
            }

            if (request.IsDefault.HasValue)
            {
                workspace.IsDefault = request.IsDefault.Value;
            }

            workspace.UpdatedAt = DateTimeOffset.UtcNow;

            _uow.Workspaces.Update(workspace);
            await _uow.SaveChangesAsync(cancellationToken);

            // Parse LayoutJson string back to JsonElement for response
            JsonElement? layoutJsonElement = null;
            if (!string.IsNullOrEmpty(workspace.LayoutJson))
            {
                using var doc = JsonDocument.Parse(workspace.LayoutJson);
                layoutJsonElement = doc.RootElement.Clone();
            }

            var workspaceDto = new WorkspaceDto
            {
                Id = workspace.Id,
                WorkspaceName = workspace.WorkspaceName,
                LayoutJson = layoutJsonElement,
                IsDefault = workspace.IsDefault,
                ShareCode = workspace.ShareCode
            };

            _logger.LogInformation("Workspace updated successfully: {WorkspaceId} by user: {UserId}", workspace.Id, _currentUserService.UserId);
            return ApiResponse<WorkspaceDto>.Success(workspaceDto, "Cập nhật workspace thành công.");
        }
    }
}
