using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Commands.CreateWatchList;

public class CreateWatchListCommandHandler : IRequestHandler<CreateWatchListCommand, ApiResponse<WatchListDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtService _jwtService;
    private readonly ILogger<CreateWatchListCommandHandler> _logger;

    public CreateWatchListCommandHandler(
        IUnitOfWork uow,
        IHttpContextAccessor httpContextAccessor,
        IJwtService jwtService,
        ILogger<CreateWatchListCommandHandler> logger)
    {
        _uow = uow;
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<ApiResponse<WatchListDto>> Handle(
        CreateWatchListCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext ?? throw new UnauthenticatedException("Không tìm thấy HTTP context.");

            var authHeaderValue = httpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeaderValue) || !authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthenticatedException("Không tìm thấy Authorization header.");
            }

            var accessToken = authHeaderValue.Substring("Bearer ".Length).Trim();

            var tokenInfo = _jwtService.GetTokenInfo(accessToken) ?? throw new UnauthenticatedException("Access token không hợp lệ.");

            var user = await _uow.Users.GetByIdAsync(tokenInfo.UserId, cancellationToken) ?? throw new NotFoundException("Người dùng không tồn tại.");

            var nameExists = await _uow.WatchLists.ExistsByNameAsync(user.Id, request.Name.Trim(), cancellationToken);
            if (nameExists)
            {
                return ApiResponse<WatchListDto>.Failure("Tên watchlist đã tồn tại. Vui lòng chọn tên khác.");
            }

            var tickersString = JsonSerializer.Serialize(request.Tickers);

            var watchList = new WatchList
            {
                UserId = user.Id,
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

            _logger.LogInformation("Watch list created successfully {WatchListId} for user {UserId}", watchList.Id, user.Id);

            return ApiResponse<WatchListDto>.Success(result, "Tạo watchlist thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating watch list");
            throw;
        }
    }
}
