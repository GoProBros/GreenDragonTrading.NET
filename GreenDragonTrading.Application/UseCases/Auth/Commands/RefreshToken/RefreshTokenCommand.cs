using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.RefreshToken
{
    public record RefreshTokenCommand(
        string RefreshToken,
        Guid UserId
    ) : IRequest<ApiResponse<AuthResponse>>;
}
