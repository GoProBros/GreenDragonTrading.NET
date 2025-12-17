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

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResponse>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IJwtService _jwtService;
        private readonly IRedisService _redisService;
        private readonly ILogger<LoginCommandHandler> _logger;
        private readonly JwtOptions _jwtOptions;

        public LoginCommandHandler(
            IUnitOfWork uow,
            IJwtService jwtService,
            IRedisService redisService,
            ILogger<LoginCommandHandler> logger,
            IOptions<JwtOptions> jwtOptions)
        {
            _uow = uow;
            _jwtService = jwtService;
            _redisService = redisService;
            _logger = logger;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<ApiResponse<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken);
                if (user == null)
                {
                    throw new NotFoundException("Email không tồn tại.");
                }

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
                {
                    throw new UnauthenticatedException("Mật khẩu không đúng");
                }

                if (!user.IsEmailVerified)
                {
                    throw new BusinessRuleException("Email chưa được xác thực");
                }

                // Get current subscription level
                var subscriptionLevel = await _uow.UserSubscriptions.GetActiveSubscriptionAsync(user.Id, cancellationToken);

                var accessToken = _jwtService.GenerateAccessToken(
                    user.Id,
                    user.Email,
                    user.Username,
                    user.PhoneNumber,
                    user.Role.ToString(),
                    subscriptionLevel?.Subscription.LevelOrder.GetDisplayName() ?? SubscriptionLevel.Free.GetDisplayName()
                );
                var refreshToken = _jwtService.GenerateRefreshToken();

                var redisKey = $"refresh:token:{refreshToken}";
                await _redisService.SetAsync(redisKey, user.Id, TimeSpan.FromDays(_jwtOptions.RefreshTokenExpirationDays));

                var response = new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.Username,
                        PhoneNumber = user.PhoneNumber,
                        Role = user.Role.GetDisplayName(),
                        IsEmailVerified = user.IsEmailVerified,
                        SubscriptionLevel = subscriptionLevel?.Subscription.LevelOrder.GetDisplayName() ?? SubscriptionLevel.Free.GetDisplayName()
                    }
                };

                _logger.LogInformation("Người dùng đăng nhập thành công: {Email}", request.Email);
                return ApiResponse<AuthResponse>.Success(response,"Đăng nhập thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi đăng nhập: {Email}", request.Email);
                throw;
            }
        }
    }
}
