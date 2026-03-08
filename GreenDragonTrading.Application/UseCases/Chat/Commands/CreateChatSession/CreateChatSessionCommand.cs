using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.CreateChatSession
{
    public record CreateChatSessionCommand(
        string? Title = null
    ) : IRequest<ApiResponse<ChatSessionDto>>;
}
