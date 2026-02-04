using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetAllTickers;

/// <summary>
/// Query to retrieve all stock ticker symbols
/// </summary>
public class GetAllTickersQuery : IRequest<ApiResponse<List<string>>>
{
}
