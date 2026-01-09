using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Commands.DeleteWatchList;

public class DeleteWatchListCommandHandler : IRequestHandler<DeleteWatchListCommand, ApiResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtService _jwtService;
    private readonly ILogger<DeleteWatchListCommandHandler> _logger;

    public DeleteWatchListCommandHandler(
        IUnitOfWork uow,
        IHttpContextAccessor httpContextAccessor,
        IJwtService jwtService,
        ILogger<DeleteWatchListCommandHandler> logger)
    {
        _uow = uow;
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<ApiResponse> Handle(
        DeleteWatchListCommand request,
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
                return ApiResponse.Failure("Không tìm thấy watchlist hoặc bạn không có quyền xóa.");
            }

            _uow.WatchLists.Delete(watchList);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Watch list deleted successfully {WatchListId} for user {UserId}", request.Id, user.Id);

            return ApiResponse.Success("Xóa watchlist thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting watch list {WatchListId}", request.Id);
            throw;
        }
    }
}
