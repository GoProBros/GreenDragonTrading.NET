using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.DeleteWorkspace
{
    /// <summary>
    /// Handler for deleting a workspace
    /// </summary>
    public class DeleteWorkspaceCommandHandler : IRequestHandler<DeleteWorkspaceCommand, ApiResponse>
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtService _jwtService;
        private readonly ILogger<DeleteWorkspaceCommandHandler> _logger;

        public DeleteWorkspaceCommandHandler(
            IUnitOfWork uow,
            IHttpContextAccessor httpContextAccessor,
            IJwtService jwtService,
            ILogger<DeleteWorkspaceCommandHandler> logger)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(DeleteWorkspaceCommand request, CancellationToken cancellationToken)
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

            if (userId == null)
            {
                throw new UnauthenticatedException("Bạn cần đăng nhập để thực hiện thao tác này.");
            }

            var workspace = await _uow.Workspaces.GetByIdAsync(request.WorkspaceId, cancellationToken);
            if (workspace == null)
            {
                throw new NotFoundException("Không tìm thấy workspace.");
            }

            if (workspace.UserId != userId)
            {
                throw new AccessDeniedException("Bạn không có quyền xóa workspace này.");
            }

            _uow.Workspaces.Remove(workspace);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Workspace deleted successfully: {WorkspaceId} by user: {UserId}", request.WorkspaceId, userId);
            return ApiResponse.Success("Xóa workspace thành công.");
        }
    }
}
