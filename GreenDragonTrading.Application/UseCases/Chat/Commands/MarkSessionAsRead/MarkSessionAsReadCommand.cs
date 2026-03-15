using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.MarkSessionAsRead;

public record MarkSessionAsReadCommand(int SessionId) : IRequest<ApiResponse>;
