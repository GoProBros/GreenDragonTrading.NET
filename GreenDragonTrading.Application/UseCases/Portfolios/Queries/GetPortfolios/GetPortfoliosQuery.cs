using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Queries.GetPortfolios;

public enum PortfolioOverallFilter
{
	Profit = 1,
	Loss = 2
}

public record GetPortfoliosQuery(
	Guid? UserId = null,
	string? Ticker = null,
	PortfolioOverallFilter? OverallFilter = null,
	CommonStatus? Status = null
) : PaginationQuery, IRequest<ApiResponse<PortfolioListResponseDto>>;
