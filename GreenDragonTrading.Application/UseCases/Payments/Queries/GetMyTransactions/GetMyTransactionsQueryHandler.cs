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
    : IRequestHandler<GetMyTransactionsQuery, ApiResponse<PaginatedResponse<PaymentTransactionDto>>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ILogger<GetMyTransactionsQueryHandler> _logger = logger;

    public async Task<ApiResponse<PaginatedResponse<PaymentTransactionDto>>> Handle(
        GetMyTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        if (!string.Equals(_currentUserService.Role, nameof(UserRole.User), StringComparison.OrdinalIgnoreCase))
        {
            throw new AccessDeniedException("Chỉ người dùng role User mới được xem danh sách giao dịch.");
        }

        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var (transactions, totalCount) = await _uow.Transactions.GetPaginatedByUserIdAsync(
            userId,
            request.Status,
            request.PaymentProvider,
            pageIndex,
            pageSize,
            cancellationToken);

        var items = transactions
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

        var paginatedResponse = PaginatedResponse<PaymentTransactionDto>.Create(
            items,
            totalCount,
            pageIndex,
            pageSize);

        _logger.LogInformation(
            "Retrieved transaction history for user {UserId}: Total={TotalCount}, Returned={ReturnedCount}, Status={Status}, PaymentProvider={PaymentProvider}, PageIndex={PageIndex}, PageSize={PageSize}",
            userId,
            totalCount,
            items.Count,
            request.Status,
            request.PaymentProvider,
            pageIndex,
            pageSize);

        return ApiResponse<PaginatedResponse<PaymentTransactionDto>>.Success(
            paginatedResponse,
            "Lấy danh sách giao dịch thành công.");
    }
}
