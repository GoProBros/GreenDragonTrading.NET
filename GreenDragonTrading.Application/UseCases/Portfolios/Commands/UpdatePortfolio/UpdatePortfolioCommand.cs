using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Commands.UpdatePortfolio;

public record UpdatePortfolioCommand(
    int Id,
    string? Name = null,
    string? Description = null,
    CommonStatus? Status = null
) : IRequest<ApiResponse<PortfolioDto>>;
