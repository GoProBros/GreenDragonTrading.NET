using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Queries.GetPortfolios;

public record GetPortfoliosQuery(Guid? UserId = null) : IRequest<ApiResponse<List<PortfolioDto>>>;
