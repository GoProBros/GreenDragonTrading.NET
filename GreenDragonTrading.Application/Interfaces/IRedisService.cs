namespace GreenDragonTrading.Application.Interfaces;

public interface IRedisService
{
    Task SetRefreshTokenAsync(Guid userId, string refreshToken, TimeSpan expiration);
    Task<string?> GetRefreshTokenAsync(Guid userId);
    Task RemoveRefreshTokenAsync(Guid userId);
    Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken);
}
