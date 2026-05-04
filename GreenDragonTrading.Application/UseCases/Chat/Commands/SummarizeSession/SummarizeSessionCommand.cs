using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Chat.Commands.SummarizeSession;

public record SummarizeSessionCommand(int SessionId) : IRequest<ApiResponse<SummarizeSessionResponseDto>>;
