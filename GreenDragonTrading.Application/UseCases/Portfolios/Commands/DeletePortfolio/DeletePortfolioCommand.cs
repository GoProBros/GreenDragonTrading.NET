using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Commands.DeletePortfolio;

public record DeletePortfolioCommand(int Id) : IRequest<ApiResponse>;
