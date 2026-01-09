using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Logout
{
    /// <summary>
    /// Command to logout user and revoke tokens
    /// </summary>
    public record LogoutCommand(
        string RefreshToken,
        string AccessToken
    ) : IRequest<ApiResponse>;
}
