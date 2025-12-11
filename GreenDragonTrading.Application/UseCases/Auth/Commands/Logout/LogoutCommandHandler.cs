using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    {
        private readonly IRedisService _redisService;
        private readonly ILogger<LogoutCommandHandler> _logger;

        public LogoutCommandHandler(
            IRedisService redisService,
            ILogger<LogoutCommandHandler> logger)
        {
            _redisService = redisService;
            _logger = logger;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tokenKey = $"refresh:token:{request.RefreshToken}";
                var userId = await _redisService.GetAsync<Guid>(tokenKey);

                if (userId == Guid.Empty)
                {
                    throw new UnauthenticatedException("Refresh token không hợp lệ hoặc đã hết hạn.");
                }

                await _redisService.RemoveAsync(tokenKey);

                _logger.LogInformation("Người dùng đăng xuất thành công: {UserId}", userId);
                return new Result(true, "Người dùng đăng xuất thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống.");
                throw;
            }
        }
    }
}
