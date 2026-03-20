using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.SendSystemNotification
{
    public record SendSystemNotificationCommand(Guid? UserId, bool SendToAll, string Message)
        : IRequest<ApiResponse<SendSystemNotificationResponseDto>>;
}
