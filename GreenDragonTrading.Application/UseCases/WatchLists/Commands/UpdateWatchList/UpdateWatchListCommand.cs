using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Commands.UpdateWatchList;

public record UpdateWatchListCommand(
    int Id,
    string? Name = null,
    JsonElement? Tickers = null
) : IRequest<ApiResponse<WatchListDto>>;
