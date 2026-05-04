using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Queries.GetWatchListById;

public record GetWatchListByIdQuery(int Id) : IRequest<ApiResponse<WatchListDto>>;
