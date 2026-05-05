using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.MarkAllSessionsAsRead;

/// <summary>
/// Marks all chat sessions as read for the current user by updating LastReadAt on every participant record.
/// </summary>
public record MarkAllSessionsAsReadCommand : IRequest<ApiResponse>;
