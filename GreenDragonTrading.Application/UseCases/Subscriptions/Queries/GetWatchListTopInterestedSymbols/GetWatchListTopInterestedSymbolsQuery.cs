using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetWatchListTopInterestedSymbols;

/// <summary>
/// Query for retrieving top interested symbols from active users' watch lists.
/// </summary>
public record GetWatchListTopInterestedSymbolsQuery : IRequest<ApiResponse<WatchListTopInterestedSymbolsDto>>;
