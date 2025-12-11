using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.VerifyEmail
{
    public record VerifyEmailCommand(
        string Token
    ) : IRequest<Result>;
}
