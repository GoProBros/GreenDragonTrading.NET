using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Auth.Queries.GetMe
{
    public class GetMeQueryHandler : IRequestHandler<GetMeQuery, ApiResponse<UserDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GetMeQueryHandler> _logger;

        public GetMeQueryHandler(
            IUnitOfWork uow,
            IJwtService jwtService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<GetMeQueryHandler> logger)
        {
            _uow = uow;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ApiResponse<UserDto>> Handle(GetMeQuery request, CancellationToken cancellationToken)
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

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.Username,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role.GetDisplayName(),
                    IsEmailVerified = user.IsEmailVerified
                };

                _logger.LogInformation("Lấy thông tin người dùng thành công: {UserId}", tokenInfo.UserId);
                return ApiResponse<UserDto>.Success(userDto, "Lấy thông tin người dùng thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin người dùng.");
                throw;
            }
        }
    }
}
