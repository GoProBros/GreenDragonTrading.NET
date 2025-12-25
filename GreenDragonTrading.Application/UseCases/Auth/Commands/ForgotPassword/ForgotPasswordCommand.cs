using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.ForgotPassword
{
    public record ForgotPasswordCommand(
        string Email
    ) : IRequest<ApiResponse>;
}
