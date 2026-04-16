using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Commands.UpdateMyInvestmentCapital;

public record UpdateMyInvestmentCapitalCommand(decimal InvestmentCapital)
    : IRequest<ApiResponse<UserInvestmentCapitalDto>>;