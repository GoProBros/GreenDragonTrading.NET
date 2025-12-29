using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Commands.DeleteLayout;
public class DeleteLayoutCommandHandler : IRequestHandler<DeleteLayoutCommand, ApiResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtService _jwtService;
    private readonly ILogger<DeleteLayoutCommandHandler> _logger;

    public DeleteLayoutCommandHandler(
        IUnitOfWork uow,
        IHttpContextAccessor httpContextAccessor,
        IJwtService jwtService,
        ILogger<DeleteLayoutCommandHandler> logger)
    {
        _uow = uow;
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<ApiResponse> Handle(
        DeleteLayoutCommand request,
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

            var tokenInfo = _jwtService.GetTokenInfo(accessToken);
            if (tokenInfo == null)
            {
                throw new UnauthenticatedException("Access token không hợp lệ.");
            }

            var user = await _uow.Users.GetByIdAsync(tokenInfo.UserId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("Người dùng không tồn tại.");
            }

            var layout = await _uow.ModuleLayouts.GetByIdAsync(request.Id, cancellationToken);

            if (layout == null)
            {
                return ApiResponse.Failure(
                    "Không tìm thấy layout. Layout không tồn tại.");
            }

            if (layout.UserId.HasValue && layout.UserId.Value != user.Id)
            {
                return ApiResponse.Failure(
                    "Không có quyền. Bạn không có quyền xóa layout này.");
            }

            if (layout.IsSystemDefault && layout.UserId == null)
            {
                return ApiResponse.Failure(
                    "Không thể xóa layout hệ thống.");
            }

            _uow.ModuleLayouts.Delete(layout);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Layout deleted successfully {LayoutId} for user {UserId}", request.Id, user.Id);

            return ApiResponse.Success("Xóa layout thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting layout {LayoutId}", request.Id);
            throw;
        }
    }
}
