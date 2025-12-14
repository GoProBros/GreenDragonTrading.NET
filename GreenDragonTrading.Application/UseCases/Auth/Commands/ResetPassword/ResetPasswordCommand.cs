using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.ResetPassword
{
    public record ResetPasswordCommand(
        string Email,
        string ResetToken,
        string NewPassword
    ) : IRequest<ApiResponse>;
}
