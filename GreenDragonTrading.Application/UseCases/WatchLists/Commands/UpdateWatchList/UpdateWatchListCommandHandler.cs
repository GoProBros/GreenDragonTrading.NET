using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Commands.UpdateWatchList;

/// <summary>
/// Handler for UpdateWatchListCommand
/// </summary>
public class UpdateWatchListCommandHandler : IRequestHandler<UpdateWatchListCommand, ApiResponse<WatchListDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateWatchListCommandHandler> _logger;

    public UpdateWatchListCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<UpdateWatchListCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<WatchListDto>> Handle(
        UpdateWatchListCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var watchList = await _uow.WatchLists.GetByIdAndUserIdAsync(request.Id, userId, cancellationToken);
        if (watchList == null)
        {
            throw new NotFoundException("Watchlist không tồn tại hoặc bạn không có quyền cập nhật.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Trim() != watchList.Name)
        {
            var nameExists = await _uow.WatchLists.ExistsByNameAsync(userId, request.Name.Trim(), request.Id, cancellationToken);
            if (nameExists)
            {
                throw new ConflictException("Tên watchlist đã tồn tại. Vui lòng chọn tên khác.");
            }
            watchList.Name = request.Name.Trim();
        }

        if (request.Tickers.HasValue)
        {
            watchList.Tickers = JsonSerializer.Serialize(request.Tickers.Value);
        }

        watchList.UpdatedAt = DateTimeOffset.UtcNow;

        await _uow.SaveChangesAsync(cancellationToken);

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

        _logger.LogInformation("Watch list updated successfully {WatchListId} for user {UserId}", request.Id, userId);

        return ApiResponse<WatchListDto>.Success(result, "Cập nhật watchlist thành công");
    }
}
