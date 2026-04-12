using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Queries.GetPortfolioById;

public record GetPortfolioByIdQuery(int Id) : IRequest<ApiResponse<PortfolioDto>>;
