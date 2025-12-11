using System.Security.Claims;

namespace GreenDragonTrading.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(Guid userId, string email, string? fullName, string? phone, string role);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token);
    }
}
