using GreenDragonTrading.Application.DTOs.Auth;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponse>;
