using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.GetOrCreateDirectSession
{
    /// <summary>
    /// Gets an existing Direct session between the current user and the target user,
    /// or creates a new one if none exists.
    /// </summary>
    public record GetOrCreateDirectSessionCommand(string PhoneOrEmail)
        : IRequest<ApiResponse<DirectChatSessionDto>>;
}
