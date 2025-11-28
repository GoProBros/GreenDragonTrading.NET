namespace GreenDragonTrading.Application.DTOs.Auth;

public record RegisterRequest(
    string Username,
    string Email,
    string Password
);
