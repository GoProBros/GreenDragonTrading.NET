using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Chat.Queries.GetChatMessages
{

    public record GetChatMessagesQuery(int SessionId) : IRequest<ApiResponse<ChatSessionDetailDto>>;
}
