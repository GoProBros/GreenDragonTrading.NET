using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Chat.Queries.GetDirectChatSessions
{
    /// <summary>
    /// Returns all Direct (1-1) chat sessions for the current user.
    /// </summary>
    public record GetDirectChatSessionsQuery : IRequest<ApiResponse<List<DirectSessionListItemDto>>>;
}
