using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.SendDirectMessage
{
    /// <summary>
    /// Sends a message in a Direct (1-1) chat session.
    /// </summary>
    public record SendDirectMessageCommand(int SessionId, string Content)
        : IRequest<ApiResponse<DirectMessageDto>>;
}
