using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Payments.Queries.GetMyTransactions;

public record GetMyTransactionsQuery : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<PaymentTransactionDto>>>
{
	public TransactionStatus? Status { get; init; }
	public PaymentType? PaymentProvider { get; init; }
}
