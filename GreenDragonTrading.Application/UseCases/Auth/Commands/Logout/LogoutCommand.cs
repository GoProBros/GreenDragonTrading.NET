using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Logout
{
    public record LogoutCommand(string RefreshToken) : IRequest<Result>;
}
