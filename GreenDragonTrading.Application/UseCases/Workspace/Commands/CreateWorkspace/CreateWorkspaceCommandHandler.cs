using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.CreateWorkspace
{
    public class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, ApiResponse<WorkspaceDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtService _jwtService;
        private readonly ILogger<CreateWorkspaceCommandHandler> _logger;

        public CreateWorkspaceCommandHandler(
            IUnitOfWork uow,
            IHttpContextAccessor httpContextAccessor,
            IJwtService jwtService,
            ILogger<CreateWorkspaceCommandHandler> logger)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ApiResponse<WorkspaceDto>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var authHeader = httpContext?.Request.Headers["Authorization"].ToString();
                
                Guid? userId = null;
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    var tokenInfo = _jwtService.GetTokenInfo(token);
                    userId = tokenInfo?.UserId;
                }

                var shareCode = await GenerateUniqueShareCodeAsync(cancellationToken);

                var workspace = new Domain.Entities.Workspace
                {
                    UserId = userId,
                    WorkspaceName = request.WorkspaceName,
                    LayoutJson = request.LayoutJson,
                    IsDefault = request.IsDefault,
                    ShareCode = shareCode,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdateAt = DateTimeOffset.UtcNow
                };

                await _uow.Workspaces.AddAsync(workspace, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);

                var workspaceDto = new WorkspaceDto
                {
                    Id = workspace.Id,
                    WorkspaceName = workspace.WorkspaceName,
                    LayoutJson = workspace.LayoutJson,
                    IsDefault = workspace.IsDefault,
                    ShareCode = workspace.ShareCode
                };

                _logger.LogInformation("Tạo workspace thành công: {WorkspaceId} cho user: {UserId}", workspace.Id, userId);
                return ApiResponse<WorkspaceDto>.Success(workspaceDto, "Tạo workspace thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo workspace");
                throw;
            }
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
