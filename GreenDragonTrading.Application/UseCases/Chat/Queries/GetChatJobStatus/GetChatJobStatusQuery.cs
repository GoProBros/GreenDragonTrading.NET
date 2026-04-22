using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Chat.Queries.GetChatJobStatus
{
    public record GetChatJobStatusQuery(string JobId) : IRequest<ApiResponse<ChatAsyncJobStatusDto>>;
}
