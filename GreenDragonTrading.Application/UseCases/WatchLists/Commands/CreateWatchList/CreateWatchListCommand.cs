using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Commands.CreateWatchList;

public record CreateWatchListCommand(
    string Name,
    JsonElement Tickers
) : IRequest<ApiResponse<WatchListDto>>;
