using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Telegram.Queries.GenerateTelegramStartToken;

public record GenerateTelegramStartTokenQuery : IRequest<ApiResponse<TelegramStartLinkDto>>;