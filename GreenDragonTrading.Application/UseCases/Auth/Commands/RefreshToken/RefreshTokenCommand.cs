using GreenDragonTrading.Application.DTOs.Auth;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken
) : IRequest<AuthResponse>;
