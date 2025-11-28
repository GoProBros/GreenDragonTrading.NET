using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);
    int GetAccessTokenExpirationMinutes();
    int GetRefreshTokenExpirationDays();
}
