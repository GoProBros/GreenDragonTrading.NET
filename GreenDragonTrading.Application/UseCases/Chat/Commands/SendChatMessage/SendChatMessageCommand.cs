using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.SendChatMessage
{
    public record SendChatMessageCommand(
        int SessionId,
        string Message
    ) : IRequest<ApiResponse<SendChatMessageResponseDto>>;
}
