using GreenDragonTrading.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace GreenDragonTrading.Infrastructure.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<TokenBlacklistService> _logger;
        private const string BlacklistKeyPrefix = "blacklist:token:";

        public TokenBlacklistService(
            IConnectionMultiplexer redis,
            ILogger<TokenBlacklistService> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task BlacklistTokenAsync(string jti, DateTime expiresAt, CancellationToken cancellationToken = default)
        {
            try
            {
                var db = _redis.GetDatabase();
                var key = $"{BlacklistKeyPrefix}{jti}";
                var timeRemaining = expiresAt - DateTime.UtcNow;

                if (timeRemaining.TotalSeconds > 0)
                {
                    await db.StringSetAsync(key, "blacklisted", timeRemaining);
                    _logger.LogInformation("Token {jti} đã được blacklist trong {TimeRemaining} giây", jti, timeRemaining.TotalSeconds);
                }
                else
                {
                    _logger.LogWarning("Token {jti} đã hết hạn, bỏ qua blacklist", jti);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi blacklist token {jti}", jti);
                throw;
            }
        }

        public async Task<bool> IsTokenBlacklistedAsync(string jti, CancellationToken cancellationToken = default)
        {
            try
            {
                var db = _redis.GetDatabase();
                var key = $"{BlacklistKeyPrefix}{jti}";
                var exists = await db.KeyExistsAsync(key);
                
                if (exists)
                {
                    _logger.LogDebug("Token {jti} đã bị blacklist", jti);
                }
                
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra blacklist cho token {jti}", jti);
                return true;
            }
        }
    }
}
