using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Queries.GetWatchListById;

public class GetWatchListByIdQueryHandler : IRequestHandler<GetWatchListByIdQuery, ApiResponse<WatchListDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtService _jwtService;
    private readonly ILogger<GetWatchListByIdQueryHandler> _logger;

    public GetWatchListByIdQueryHandler(
        IUnitOfWork uow,
        IHttpContextAccessor httpContextAccessor,
        IJwtService jwtService,
        ILogger<GetWatchListByIdQueryHandler> logger)
    {
        _uow = uow;
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<ApiResponse<WatchListDto>> Handle(
        GetWatchListByIdQuery request,
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

            var watchList = await _uow.WatchLists.GetByIdAndUserIdAsync(request.Id, user.Id, cancellationToken);

            if (watchList == null)
            {
                return ApiResponse<WatchListDto>.Failure("Không tìm thấy watchlist hoặc bạn không có quyền truy cập.");
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

            _logger.LogInformation("Successfully retrieved watch list {WatchListId} for user {UserId}", request.Id, user.Id);

            return ApiResponse<WatchListDto>.Success(result, "Lấy watchlist thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving watch list {WatchListId}", request.Id);
            throw;
        }
    }
}
