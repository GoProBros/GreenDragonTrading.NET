using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Payments.Queries.GetMyTransactions;

public record GetMyTransactionsQuery : IRequest<ApiResponse<List<PaymentTransactionDto>>>;
