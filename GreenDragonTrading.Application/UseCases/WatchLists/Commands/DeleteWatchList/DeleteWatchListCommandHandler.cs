using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Commands.DeleteWatchList;

/// <summary>
/// Handler for DeleteWatchListCommand
/// </summary>
public class DeleteWatchListCommandHandler : IRequestHandler<DeleteWatchListCommand, ApiResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteWatchListCommandHandler> _logger;

    public DeleteWatchListCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<DeleteWatchListCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse> Handle(
        DeleteWatchListCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var watchList = await _uow.WatchLists.GetByIdAndUserIdAsync(request.Id, userId, cancellationToken);
        if (watchList == null)
        {
            throw new NotFoundException("Watchlist không tồn tại hoặc bạn không có quyền xóa.");
        }

        _uow.WatchLists.Delete(watchList);
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Watch list deleted successfully {WatchListId} for user {UserId}", request.Id, userId);

        return ApiResponse.Success("Xóa watchlist thành công");
    }
}
