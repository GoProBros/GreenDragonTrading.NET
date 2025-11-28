using GreenDragonTrading.Application.Interfaces;
using StackExchange.Redis;

namespace GreenDragonTrading.Infrastructure.Services;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private const string RefreshTokenPrefix = "refresh_token:";

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task SetRefreshTokenAsync(Guid userId, string refreshToken, TimeSpan expiration)
    {
        var key = $"{RefreshTokenPrefix}{userId}";
        await _db.StringSetAsync(key, refreshToken, expiration);
    }

    public async Task<string?> GetRefreshTokenAsync(Guid userId)
    {
        var key = $"{RefreshTokenPrefix}{userId}";
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task RemoveRefreshTokenAsync(Guid userId)
    {
        var key = $"{RefreshTokenPrefix}{userId}";
        await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var storedToken = await GetRefreshTokenAsync(userId);
        return storedToken != null && storedToken == refreshToken;
    }
}
