using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Queries.GetMyWatchLists;

public record GetMyWatchListsQuery() : IRequest<ApiResponse<List<WatchListListItemDto>>>;
