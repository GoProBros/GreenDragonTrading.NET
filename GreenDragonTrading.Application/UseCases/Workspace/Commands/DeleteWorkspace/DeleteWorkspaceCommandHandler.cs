using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.DeleteWorkspace
{
    /// <summary>
    /// Handler for deleting a workspace
    /// </summary>
    public class DeleteWorkspaceCommandHandler : IRequestHandler<DeleteWorkspaceCommand, ApiResponse>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteWorkspaceCommandHandler> _logger;

        public DeleteWorkspaceCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<DeleteWorkspaceCommandHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(DeleteWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetRequiredUserId();

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
