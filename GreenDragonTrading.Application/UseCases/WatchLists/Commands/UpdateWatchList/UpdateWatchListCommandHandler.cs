using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Commands.UpdateWatchList;

public class UpdateWatchListCommandHandler : IRequestHandler<UpdateWatchListCommand, ApiResponse<WatchListDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtService _jwtService;
    private readonly ILogger<UpdateWatchListCommandHandler> _logger;

    public UpdateWatchListCommandHandler(
        IUnitOfWork uow,
        IHttpContextAccessor httpContextAccessor,
        IJwtService jwtService,
        ILogger<UpdateWatchListCommandHandler> logger)
    {
        _uow = uow;
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<ApiResponse<WatchListDto>> Handle(
        UpdateWatchListCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new UnauthenticatedException("Không tìm thấy HTTP context.");
            }

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
                return ApiResponse<WatchListDto>.Failure("Không tìm thấy watchlist hoặc bạn không có quyền cập nhật.");
            }

            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name.Trim() != watchList.Name)
            {
                var nameExists = await _uow.WatchLists.ExistsByNameAsync(user.Id, request.Name.Trim(), request.Id, cancellationToken);
                if (nameExists)
                {
                    return ApiResponse<WatchListDto>.Failure("Tên watchlist đã tồn tại. Vui lòng chọn tên khác.");
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

            _logger.LogInformation("Watch list updated successfully {WatchListId} for user {UserId}", request.Id, user.Id);

            return ApiResponse<WatchListDto>.Success(result, "Cập nhật watchlist thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating watch list {WatchListId}", request.Id);
            throw;
        }
    }
}
