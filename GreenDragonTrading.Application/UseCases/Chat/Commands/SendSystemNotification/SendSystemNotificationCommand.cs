using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.SendSystemNotification
{
    public record SendSystemNotificationCommand(List<Guid>? UserIds, bool SendToAll, string Message)
        : IRequest<ApiResponse<SendSystemNotificationResponseDto>>;
}
