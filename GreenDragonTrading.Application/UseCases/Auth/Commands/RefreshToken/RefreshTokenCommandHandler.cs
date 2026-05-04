using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponse>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IJwtService _jwtService;
        private readonly IRedisService _redisService;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;
        private readonly JwtOptions _jwtOptions;

        public RefreshTokenCommandHandler(
            IUnitOfWork uow,
            IJwtService jwtService,
            IRedisService redisService,
            ILogger<RefreshTokenCommandHandler> logger,
            IOptions<JwtOptions> jwtOptions)
        {
            _uow = uow;
            _jwtService = jwtService;
            _redisService = redisService;
            _logger = logger;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<ApiResponse<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tokenKey = $"refresh:token:{request.RefreshToken}";
                var userId = await _redisService.GetAsync<Guid>(tokenKey);

                if (userId == Guid.Empty)
                {
                    throw new UnauthenticatedException("Refresh token không hợp lệ hoặc đã hết hạn.");
                }
                
                var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);

                if (user == null)
                {
                    throw new NotFoundException("Người dùng không tồn tại.");
                }

                if(userId != request.UserId)
                {
                    throw new UnauthenticatedException("Refresh token không hợp lệ cho người dùng này.");
                }

                var subscriptionLevel = await _uow.UserSubscriptions.GetActiveSubscriptionAsync(user.Id, cancellationToken);

                var accessToken = _jwtService.GenerateAccessToken(
                    user.Id,
                    user.Email,
                    user.Username,
                    user.PhoneNumber,
                    user.Role.ToString(),
                    subscriptionLevel?.Subscription.LevelOrder.ToString() ?? SubscriptionLevel.Free.ToString()
                );
                var newRefreshToken = _jwtService.GenerateRefreshToken();
          
                await _redisService.RemoveAsync(tokenKey);
                var newTokenKey = $"refresh:token:{newRefreshToken}";
                await _redisService.SetAsync(newTokenKey, userId, TimeSpan.FromDays(_jwtOptions.RefreshTokenExpirationDays));

                var response = new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.Username,
                        PhoneNumber = user.PhoneNumber,
                        Role = user.Role.GetDisplayName(),
                        IsEmailVerified = user.IsEmailVerified,
                        SubscriptionLevel = subscriptionLevel?.Subscription.LevelOrder.GetDisplayName() ?? SubscriptionLevel.Free.GetDisplayName(),
                        TelegramChatId = user.TelegramId,
                        IsTelegramLinked = !string.IsNullOrWhiteSpace(user.TelegramId)
                    }
                };

                _logger.LogInformation("Refresh token updated successfully: {UserId}", userId);
                return ApiResponse<AuthResponse>.Success(response, "Làm mới token thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System error.");
                throw;
            }
        }
    }
}
