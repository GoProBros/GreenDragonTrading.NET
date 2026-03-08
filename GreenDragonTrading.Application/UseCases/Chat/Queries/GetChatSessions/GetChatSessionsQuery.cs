using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Chat.Queries.GetChatSessions
{
    public record GetChatSessionsQuery : IRequest<ApiResponse<List<ChatSessionListItemDto>>>;
}
