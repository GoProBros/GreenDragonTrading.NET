using GreenDragonTrading.Application.Common.Models;
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
            // Get optional user ID (workspace can be created without authentication)
            var userId = _currentUserService.UserId;

            var shareCode = await GenerateUniqueShareCodeAsync(cancellationToken);

            var layoutJsonString = JsonSerializer.Serialize(request.LayoutJson);

            var workspace = new Domain.Entities.Workspace
            {
                UserId = userId,
                WorkspaceName = request.WorkspaceName,
                LayoutJson = layoutJsonString,
                IsDefault = request.IsDefault,
                ShareCode = shareCode,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _uow.Workspaces.AddAsync(workspace, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var workspaceDto = new WorkspaceDto
            {
                Id = workspace.Id,
                WorkspaceName = workspace.WorkspaceName,
                LayoutJson = request.LayoutJson,
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
