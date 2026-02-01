using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using System.Threading;
using Transaction = GreenDragonTrading.Domain.Entities.Transaction;

namespace GreenDragonTrading.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPayOSService _payOSService;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IPayOSService payOSService, IUnitOfWork uow, ILogger<PaymentService> logger)
        {
            _payOSService = payOSService;
            _uow = uow;
            _logger = logger;
        }

        public async Task<PaymentLinkResponse> CreateVipPaymentAsync(Guid userId, int subscriptionId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating VIP payment for UserId={UserId}, SubscriptionId={SubscriptionId}", userId, subscriptionId);

            var newSubscription = await _uow.Subscriptions.GetByIdAsync(subscriptionId, cancellationToken)
                      ?? throw new NotFoundException("Gói dịch vụ không tồn tại");

            // Check for downgrade attempt
            var currentHighestSub = await _uow.UserSubscriptions.GetActiveSubscriptionAsync(userId, cancellationToken);
            var currentLevelOrder = currentHighestSub?.Subscription.LevelOrder ?? 0;

            if (newSubscription.LevelOrder < currentLevelOrder)
            {
                _logger.LogWarning("Downgrade attempt blocked: UserId={UserId}, CurrentLevel={CurrentLevel}, RequestedLevel={RequestedLevel}",
                    userId, currentLevelOrder, newSubscription.LevelOrder);
                throw new BusinessRuleException("Không thể hạ cấp gói dịch vụ. Vui lòng chọn gói cao hơn hoặc bằng gói hiện tại.");
            }

            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var transaction = new Transaction
            {
                OrderCode = orderCode,
                UserId = userId,
                SubscriptionId = subscriptionId,
                Amount = newSubscription.Price,
                Status = TransactionStatus.Pending,
                Type = currentHighestSub == null || newSubscription.Id == currentHighestSub.SubscriptionId
                    ? TransactionType.Purchase
                    : TransactionType.Upgrade,
                Description = currentHighestSub == null || newSubscription.Id == currentHighestSub.SubscriptionId
                    ? $"Mua gói {newSubscription.Id}"
                    : $"Nâng cấp lên gói {newSubscription.Id}"
            };

            await _uow.Transactions.AddAsync(transaction, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var listItems = new List<ItemData>();

            var result = await _payOSService.CreatePaymentLinkAsync(
                orderCode,
                (int)newSubscription.Price,
                transaction.Description,
                listItems,
                "https://success.com",
                "https://cancel.com"
            );

            _logger.LogInformation("Payment link created: OrderCode={OrderCode}", orderCode);

            return new PaymentLinkResponse { CheckoutUrl = result.checkoutUrl, OrderCode = orderCode };
        }

        public async Task<WebhookUpdateResult> ProcessWebhookAsync(WebhookType webhookBody, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing PayOS webhook");

            var data = _payOSService.VerifyWebhook(webhookBody);

            var transaction = await _uow.Transactions.GetByOrderCodeAsync(data.orderCode, cancellationToken);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found for OrderCode={OrderCode}", data.orderCode);
                return new WebhookUpdateResult { IsSuccess = false, Message = "Không tìm thấy đơn hàng" };
            }

            if (transaction.Status == TransactionStatus.Completed)
            {
                _logger.LogInformation("Transaction already completed: OrderCode={OrderCode}", data.orderCode);
                return new WebhookUpdateResult { IsSuccess = true };
            }

            if (data.code == "00")
            {
                await _uow.BeginTransactionAsync(cancellationToken);
                try
                {
                    var newSubscription = await _uow.Subscriptions.GetByIdAsync(transaction.SubscriptionId, cancellationToken)
                        ?? throw new NotFoundException("Gói dịch vụ không tồn tại");

                    int durationDays = newSubscription.DurationInDays;

                    var currentHighestSub = await _uow.UserSubscriptions.GetActiveSubscriptionAsync(transaction.UserId, cancellationToken);

                    DateTimeOffset startDate;
                    DateTimeOffset endDate;

                    if (currentHighestSub == null)
                    {
                        startDate = DateTimeOffset.UtcNow;
                        endDate = startDate.AddDays(durationDays);

                        _logger.LogInformation("Case 1 - New purchase: UserId={UserId}, SubscriptionId={SubscriptionId}", 
                            transaction.UserId, transaction.SubscriptionId);
                    }
                    else if (newSubscription.Id == currentHighestSub.SubscriptionId)
                    {
                        var maxEndDate = await _uow.UserSubscriptions.GetMaxEndDateBySubscriptionIdAsync(
                            transaction.UserId, newSubscription.Id, cancellationToken);

                        startDate = maxEndDate ?? DateTimeOffset.UtcNow;
                        endDate = startDate.AddDays(durationDays);

                        _logger.LogInformation("Case 2 - Stacking: UserId={UserId}, SubscriptionId={SubscriptionId}, StartDate={StartDate}", 
                            transaction.UserId, transaction.SubscriptionId, startDate);
                    }
                    else if (newSubscription.LevelOrder > currentHighestSub.Subscription.LevelOrder)
                    {
                        startDate = DateTimeOffset.UtcNow;
                        endDate = startDate.AddDays(durationDays);

                        // Mark all active subscriptions as Upgraded
                        await _uow.UserSubscriptions.MarkAllActiveAsUpgradedAsync(transaction.UserId, cancellationToken);

                        _logger.LogInformation("Case 3 - Upgrade: UserId={UserId}, OldLevel={OldLevel}, NewLevel={NewLevel}", 
                            transaction.UserId, currentHighestSub.Subscription.LevelOrder, newSubscription.LevelOrder);
                    }
                    else
                    {
                        _logger.LogError("Downgrade attempt in webhook - this should have been blocked: UserId={UserId}", transaction.UserId);
                        throw new BusinessRuleException("Không thể hạ cấp gói dịch vụ.");
                    }

                    transaction.Status = TransactionStatus.Completed;
                    _uow.Transactions.Update(transaction);

                    var newUserSub = new UserSubscription
                    {
                        UserId = transaction.UserId,
                        SubscriptionId = transaction.SubscriptionId,
                        StartDate = startDate,
                        EndDate = endDate,
                        Status = SubscriptionStatus.Active
                    };
                    await _uow.UserSubscriptions.AddAsync(newUserSub, cancellationToken);

                    await _uow.SaveChangesAsync(cancellationToken);
                    await _uow.CommitTransactionAsync(cancellationToken);

                    _logger.LogInformation("Payment completed: OrderCode={OrderCode}, UserId={UserId}, Type={Type}", 
                        data.orderCode, transaction.UserId, transaction.Type);

                    return new WebhookUpdateResult { IsSuccess = true, OrderCode = data.orderCode };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing webhook for OrderCode={OrderCode}", data.orderCode);
                    await _uow.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            }

            return new WebhookUpdateResult { IsSuccess = false, Message = "Thanh toán thất bại" };
        }

        public async Task<PaymentStatusResponse?> GetPaymentStatusAsync(long orderCode, Guid userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting payment status for OrderCode={OrderCode}, UserId={UserId}", orderCode, userId);

            var transaction = await _uow.Transactions.GetByOrderCodeAsync(orderCode, cancellationToken);

            if (transaction == null)
            {
                throw new NotFoundException("Giao dịch không tồn tại.");
            }

            if (transaction.UserId != userId)
            {
                throw new AccessDeniedException("Bạn không có quyền xem giao dịch này.");
            }

            return new PaymentStatusResponse
            {
                OrderCode = transaction.OrderCode,
                Amount = transaction.Amount,
                Status = transaction.Status,
                StatusName = transaction.Status.GetDisplayName(),
                SubscriptionId = transaction.SubscriptionId,
                SubscriptionName = transaction.Subscription?.Name ?? "",
                CreatedAt = transaction.CreatedAt
            };
        }

        public async Task<PaymentInformationResponse> CancelPaymentAsync(long orderCode, string reason, CancellationToken cancellationToken = default)
        {
            var result = await _payOSService.CancelPaymentLink(orderCode, reason, cancellationToken);

            if (result != null)
            {
                var transaction = await _uow.Transactions.GetByOrderCodeAsync(orderCode, cancellationToken);

                if (transaction != null)
                {
                    transaction.Status = TransactionStatus.Cancelled;

                    await _uow.SaveChangesAsync(cancellationToken);
                }
                return new PaymentInformationResponse
                {
                    OrderCode = result.orderCode,
                    Amount = result.amount,
                    Status = result.status,
                    CancellationReason = result.cancellationReason,
                    CreatedAt = null
                };
            }

            throw new BusinessRuleException("Hủy thanh toán không thành công.");
        }
    }
}