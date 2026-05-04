using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Commands.CreateWatchList;

/// <summary>
/// Handler for CreateWatchListCommand
/// </summary>
public class CreateWatchListCommandHandler : IRequestHandler<CreateWatchListCommand, ApiResponse<WatchListDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateWatchListCommandHandler> _logger;

    public CreateWatchListCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<CreateWatchListCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<WatchListDto>> Handle(
        CreateWatchListCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var nameExists = await _uow.WatchLists.ExistsByNameAsync(userId, request.Name.Trim(), cancellationToken);
        if (nameExists)
        {
            throw new ConflictException("Tên watchlist đã tồn tại. Vui lòng chọn tên khác.");
        }

        var tickersString = JsonSerializer.Serialize(request.Tickers);

        var watchList = new WatchList
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Tickers = tickersString,
            Status = CommonStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _uow.WatchLists.AddAsync(watchList, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var result = new WatchListDto
        {
            Id = watchList.Id,
            Name = watchList.Name,
            Tickers = request.Tickers,
            Status = watchList.Status,
            CreatedAt = watchList.CreatedAt,
            UpdatedAt = watchList.UpdatedAt
        };

        _logger.LogInformation("Watch list created successfully {WatchListId} for user {UserId}", watchList.Id, userId);

        return ApiResponse<WatchListDto>.Success(result, "Tạo watchlist thành công");
    }
}
