using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponse>
    {
        private readonly IRedisService _redisService;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<LogoutCommandHandler> _logger;

        public LogoutCommandHandler(
            IRedisService redisService,
            ITokenBlacklistService tokenBlacklistService,
            IJwtService jwtService,
            ILogger<LogoutCommandHandler> logger)
        {
            _redisService = redisService;
            _tokenBlacklistService = tokenBlacklistService;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // xóa refresh token khỏi redis
                var tokenKey = $"refresh:token:{request.RefreshToken}";
                var userId = await _redisService.GetAsync<Guid>(tokenKey);

                if (userId == Guid.Empty)
                {
                    throw new UnauthenticatedException("Refresh token không hợp lệ hoặc đã hết hạn.");
                }

                await _redisService.RemoveAsync(tokenKey);
                _logger.LogInformation("Refresh token revoked for user: {UserId}", userId);

                // cho access token vào blacklist
                var tokenInfo = _jwtService.GetTokenInfo(request.AccessToken);
                
                if (tokenInfo == null)
                {
                    throw new BusinessRuleException("Access token không hợp lệ hoặc thiếu thông tin bắt buộc.");
                }

                await _tokenBlacklistService.BlacklistTokenAsync(tokenInfo.Jti, tokenInfo.ExpiresAt, cancellationToken);
                
                _logger.LogInformation("Access token {Jti} blacklisted for user {UserId}", tokenInfo.Jti, userId);

                return ApiResponse.Success("Đăng xuất thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout.");
                throw;
            }
        }
    }
}
