using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Commands.DeleteLayout;

/// <summary>
/// Handler for DeleteLayoutCommand
/// </summary>
public class DeleteLayoutCommandHandler : IRequestHandler<DeleteLayoutCommand, ApiResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteLayoutCommandHandler> _logger;

    public DeleteLayoutCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<DeleteLayoutCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse> Handle(
        DeleteLayoutCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var layout = await _uow.ModuleLayouts.GetByIdAsync(request.Id, cancellationToken);
        if (layout == null)
        {
            throw new NotFoundException("Layout không tồn tại.");
        }

        if (layout.UserId.HasValue && layout.UserId.Value != userId)
        {
            throw new AccessDeniedException("Bạn không có quyền xóa layout này.");
        }

        if (layout.IsSystemDefault && layout.UserId == null)
        {
            throw new AccessDeniedException("Không thể xóa layout hệ thống.");
        }

        _uow.ModuleLayouts.Delete(layout);
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Layout deleted successfully {LayoutId} for user {UserId}", request.Id, userId);

        return ApiResponse.Success("Xóa layout thành công");
    }
}
