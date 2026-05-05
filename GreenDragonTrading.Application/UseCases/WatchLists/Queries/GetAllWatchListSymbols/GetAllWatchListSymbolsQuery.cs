using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Queries.GetAllWatchListSymbols;

/// <summary>
/// Query to get all unique symbols across all watchlists of the current user.
/// </summary>
public record GetAllWatchListSymbolsQuery() : IRequest<ApiResponse<List<string>>>;
