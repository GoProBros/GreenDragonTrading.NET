using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.TradingTransactions.Commands.CreateTradingTransaction;

public record CreateTradingTransactionCommand(
    int PortfolioId = 0,
    TransactionSide? Side = null,
    decimal? Quantity = null,
    decimal? Price = null,
    DateTimeOffset? TransactionDate = null,
    string? Note = null,
    string? OriginalMessage = null
) : IRequest<ApiResponse<TradingTransactionDto>>;
