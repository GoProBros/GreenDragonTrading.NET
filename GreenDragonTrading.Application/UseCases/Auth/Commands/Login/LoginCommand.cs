using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Login
{
    public record LoginCommand(
        string Email,
        string Password
    ) : IRequest<ApiResponse<AuthResponse>>;
}
