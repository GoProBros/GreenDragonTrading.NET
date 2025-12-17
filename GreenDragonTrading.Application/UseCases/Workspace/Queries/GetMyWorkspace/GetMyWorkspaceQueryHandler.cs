using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Workspace.Queries.GetMyWorkspace
{
    public class GetMyWorkspaceQueryHandler : IRequestHandler<GetMyWorkspaceQuery, ApiResponse<List<WorkspaceDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GetMyWorkspaceQueryHandler> _logger;
        private readonly IJwtService _jwtService;

        public GetMyWorkspaceQueryHandler(
            IUnitOfWork uow,
            IHttpContextAccessor httpContextAccessor,
            IJwtService jwtService,
            ILogger<GetMyWorkspaceQueryHandler> logger)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ApiResponse<List<WorkspaceDto>>> Handle(GetMyWorkspaceQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Lấy access token từ Authorization header
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

                // Sử dụng JwtService để lấy thông tin từ token
                var tokenInfo = _jwtService.GetTokenInfo(accessToken);
                if (tokenInfo == null)
                {
                    throw new UnauthenticatedException("Access token không hợp lệ.");
                }

                // Lấy thông tin user từ database
                var user = await _uow.Users.GetByIdAsync(tokenInfo.UserId, cancellationToken);
                if (user == null)
                {
                    throw new NotFoundException("Người dùng không tồn tại.");
                }
                var workspaces = await _uow.Workspaces.GetWorkspaceByUserIdAsync(user.Id, cancellationToken);
                var result = workspaces.Select(w => new WorkspaceDto
                {
                    Id = w.Id,
                    WorkspaceName = w.WorkspaceName,
                    LayoutJson = w.LayoutJson,
                    IsDefault = w.IsDefault,
                    ShareCode = w.ShareCode
                }).ToList();

                _logger.LogInformation("Lấy thành công danh sách workspace của người dùng: {UserId}", tokenInfo.UserId);
                return ApiResponse<List<WorkspaceDto>>.Success(result, "Lấy thành công danh sách workspace của người dùng.");
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy workspace của người dùng.");
                throw;
            }
        }
    }
}
