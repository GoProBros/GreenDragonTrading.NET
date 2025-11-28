using GreenDragonTrading.Application.DTOs.Auth;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Register;

public record RegisterCommand(
    string Username,
    string Email,
    string Password
) : IRequest<AuthResponse>;
