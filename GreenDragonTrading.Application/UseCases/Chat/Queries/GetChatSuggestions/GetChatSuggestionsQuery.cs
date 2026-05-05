using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Chat.Queries.GetChatSuggestions
{
    /// <summary>
    /// Query to retrieve AI-powered chat question suggestions for the current user.
    /// </summary>
    public record GetChatSuggestionsQuery : IRequest<ApiResponse<ChatSuggestionsDto>>;
}
