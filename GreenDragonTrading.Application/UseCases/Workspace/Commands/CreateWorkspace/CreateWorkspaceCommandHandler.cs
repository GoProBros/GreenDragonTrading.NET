using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Common.Utils;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.CreateWorkspace
{
    /// <summary>
    /// Handler for CreateWorkspaceCommand
    /// </summary>
    public class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, ApiResponse<WorkspaceDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateWorkspaceCommandHandler> _logger;

        public CreateWorkspaceCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<CreateWorkspaceCommandHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<WorkspaceDto>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetRequiredUserId();

            if (!_currentUserService.IsAdminOrStaff)
            {
                var effectiveSubscription = await SubscriptionAccessHelper.GetEffectiveSubscriptionAsync(
                    _uow,
                    false,
                    userId,
                    cancellationToken);

                if (effectiveSubscription != null)
                {
                    var existingWorkspaces = await _uow.Workspaces.GetWorkspaceByUserIdAsync(
                        userId,
                        request.Type,
                        cancellationToken);

                    var maxWorkspaces = effectiveSubscription.MaxWorkspaces ?? 1;

                    if (existingWorkspaces.Count >= maxWorkspaces)
                    {
                        throw new Domain.Exceptions.BusinessRuleException(
                            $"Gói đăng ký của bạn chỉ cho phép tối đa {maxWorkspaces} workspace cho loại {request.Type}.");
                    }
                }
            }

            var shareCode = await GenerateUniqueShareCodeAsync(cancellationToken);

            var systemDefault = await _uow.Workspaces.GetSystemDefaultWorkspaceAsync(request.Type, cancellationToken);
            var layoutJsonString = systemDefault?.LayoutJson ?? "{}";

            var workspace = new Domain.Entities.Workspace
            {
                UserId = userId,
                WorkspaceName = request.WorkspaceName,
                LayoutJson = layoutJsonString,
                Type = request.Type,
                IsDefault = request.IsDefault,
                ShareCode = shareCode,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _uow.Workspaces.AddAsync(workspace, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

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
                Type = workspace.Type,
                IsDefault = workspace.IsDefault,
                ShareCode = workspace.ShareCode
            };

            _logger.LogInformation("Workspace created successfully: {WorkspaceId} for user: {UserId}", workspace.Id, userId);
            return ApiResponse<WorkspaceDto>.Success(workspaceDto, "Tạo workspace thành công.");
        }

        private async Task<string> GenerateUniqueShareCodeAsync(CancellationToken cancellationToken)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const int codeLength = 8;
            const int maxAttempts = 10;

            char[] codeBuffer = new char[codeLength];

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                for (int i = 0; i < codeLength; i++)
                {
                    codeBuffer[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
                }

                var shareCode = new string(codeBuffer);

                var exists = await _uow.Workspaces.ShareCodeExistsAsync(shareCode, cancellationToken);

                if (!exists)
                {
                    return shareCode;
                }
            }

            throw new InvalidOperationException("Hệ thống hiện không thể tạo mã chia sẻ duy nhất. Vui lòng thử lại.");
        }
    }
}
