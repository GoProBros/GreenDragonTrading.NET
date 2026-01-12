using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.UpdateWorkspace
{
    public class UpdateWorkspaceCommandHandler : IRequestHandler<UpdateWorkspaceCommand, ApiResponse<WorkspaceDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtService _jwtService;
        private readonly ILogger<UpdateWorkspaceCommandHandler> _logger;

        public UpdateWorkspaceCommandHandler(
            IUnitOfWork uow,
            IHttpContextAccessor httpContextAccessor,
            IJwtService jwtService,
            ILogger<UpdateWorkspaceCommandHandler> logger)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ApiResponse<WorkspaceDto>> Handle(UpdateWorkspaceCommand request, CancellationToken cancellationToken)
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

                var workspace = await _uow.Workspaces.GetByIdAsync(request.WorkspaceId, cancellationToken);
                if (workspace == null)
                {
                    throw new NotFoundException("Không tìm thấy workspace.");
                }

                if (workspace.UserId != userId)
                {
                    throw new UnauthenticatedException("Bạn không có quyền cập nhật workspace này.");
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

                _logger.LogInformation("Workspace updated successfully: {WorkspaceId} by user: {UserId}", workspace.Id, userId);
                return ApiResponse<WorkspaceDto>.Success(workspaceDto, "Cập nhật workspace thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workspace {WorkspaceId}", request.WorkspaceId);
                throw;
            }
        }
    }
}
