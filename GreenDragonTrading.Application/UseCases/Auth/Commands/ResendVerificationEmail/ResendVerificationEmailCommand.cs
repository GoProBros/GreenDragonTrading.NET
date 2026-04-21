using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.ResendVerificationEmail
{
    public record ResendVerificationEmailCommand(
        string Email
    ) : IRequest<ApiResponse>;
}
