namespace GreenDragonTrading.Application.DTOs.Auth;

public record AuthResponse(
    Guid UserId,
    string Username,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
