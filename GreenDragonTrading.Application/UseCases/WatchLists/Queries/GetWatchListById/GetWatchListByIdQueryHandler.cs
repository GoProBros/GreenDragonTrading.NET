using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Queries.GetWatchListById;

/// <summary>
/// Handler for GetWatchListByIdQuery
/// </summary>
public class GetWatchListByIdQueryHandler : IRequestHandler<GetWatchListByIdQuery, ApiResponse<WatchListDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetWatchListByIdQueryHandler> _logger;

    public GetWatchListByIdQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<GetWatchListByIdQueryHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<WatchListDto>> Handle(
        GetWatchListByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var watchList = await _uow.WatchLists.GetByIdAndUserIdAsync(request.Id, userId, cancellationToken);
        if (watchList == null)
        {
            throw new NotFoundException("Watchlist không tồn tại hoặc bạn không có quyền truy cập.");
        }

        JsonElement tickers;
        try
        {
            tickers = JsonSerializer.Deserialize<JsonElement>(watchList.Tickers);
        }
        catch
        {
            tickers = JsonSerializer.Deserialize<JsonElement>("[]");
        }

        var result = new WatchListDto
        {
            Id = watchList.Id,
            Name = watchList.Name,
            Tickers = tickers,
            Status = watchList.Status,
            CreatedAt = watchList.CreatedAt,
            UpdatedAt = watchList.UpdatedAt
        };

        _logger.LogInformation("Successfully retrieved watch list {WatchListId} for user {UserId}", request.Id, userId);

        return ApiResponse<WatchListDto>.Success(result, "Lấy watchlist thành công");
    }
}
