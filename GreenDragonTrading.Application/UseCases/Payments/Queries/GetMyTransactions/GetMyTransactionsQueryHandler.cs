using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Payments.Queries.GetMyTransactions;

public class GetMyTransactionsQueryHandler(
    IUnitOfWork uow,
    ICurrentUserService currentUserService,
    ILogger<GetMyTransactionsQueryHandler> logger)
    : IRequestHandler<GetMyTransactionsQuery, ApiResponse<List<PaymentTransactionDto>>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ILogger<GetMyTransactionsQueryHandler> _logger = logger;

    public async Task<ApiResponse<List<PaymentTransactionDto>>> Handle(
        GetMyTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        if (!string.Equals(_currentUserService.Role, nameof(UserRole.User), StringComparison.OrdinalIgnoreCase))
        {
            throw new AccessDeniedException("Chỉ người dùng role User mới được xem danh sách giao dịch.");
        }

        var transactions = await _uow.Transactions.GetByUserIdAsync(userId, cancellationToken);

        var items = transactions
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.OrderCode)
            .Select(t => new PaymentTransactionDto
            {
                Id = t.Id,
                OrderCode = t.OrderCode,
                SubscriptionId = t.SubscriptionId,
                SubscriptionName = t.Subscription?.Name ?? string.Empty,
                Amount = t.Amount,
                Status = t.Status,
                StatusName = t.Status.GetDisplayName(),
                Type = t.Type,
                TypeName = t.Type.GetDisplayName(),
                PaymentProvider = t.PaymentProvider,
                PaymentProviderName = t.PaymentProvider.GetDisplayName(),
                ProviderTransactionId = t.ProviderTransactionId,
                CheckoutUrl = t.CheckoutUrl,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            })
            .ToList();

        _logger.LogInformation(
            "Retrieved {TransactionCount} transactions for user {UserId}",
            items.Count,
            userId);

        return ApiResponse<List<PaymentTransactionDto>>.Success(items, "Lấy danh sách giao dịch thành công.");
    }
}
