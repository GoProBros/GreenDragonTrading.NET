using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.GoogleLogin
{
    public record GoogleLoginCommand(string IdToken) : IRequest<ApiResponse<AuthResponse>>;
}
