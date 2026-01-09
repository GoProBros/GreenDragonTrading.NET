using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Commands.DeleteWatchList;

public record DeleteWatchListCommand(int Id) : IRequest<ApiResponse>;
