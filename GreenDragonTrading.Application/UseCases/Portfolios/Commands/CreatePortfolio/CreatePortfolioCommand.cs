using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Commands.CreatePortfolio;

public record CreatePortfolioCommand(
    string Ticker,
    string? Name = null,
    string? Description = null,
    CommonStatus Status = CommonStatus.Active
) : IRequest<ApiResponse<PortfolioDto>>;
