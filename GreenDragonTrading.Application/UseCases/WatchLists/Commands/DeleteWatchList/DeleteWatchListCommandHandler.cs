using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
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
    private readonly IRedisService _redisService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteWatchListCommandHandler> _logger;

    public DeleteWatchListCommandHandler(
        IUnitOfWork uow,
        IRedisService redisService,
        ICurrentUserService currentUserService,
        ILogger<DeleteWatchListCommandHandler> logger)
    {
        _uow = uow;
        _redisService = redisService;
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

        try
        {
            await _redisService.RemoveAsync(RedisConstants.ProactiveWatchListTickerIndex());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate proactive watchlist index cache after deleting watchlist {WatchListId}", request.Id);
        }

        _logger.LogInformation("Watch list deleted successfully {WatchListId} for user {UserId}", request.Id, userId);

        return ApiResponse.Success("Xóa watchlist thành công");
    }
}
