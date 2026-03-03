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
using Transaction = GreenDragonTrading.Domain.Entities.Transaction;

namespace GreenDragonTrading.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPayOSService _payOSService;
        private readonly IMomoService _momoService;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPayOSService payOSService,
            IMomoService momoService,
            IUnitOfWork uow,
            ILogger<PaymentService> logger)
        {
            _payOSService = payOSService;
            _momoService = momoService;
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

            var transactionType = currentHighestSub == null || newSubscription.Id == currentHighestSub.SubscriptionId
                ? TransactionType.Purchase
                : TransactionType.Upgrade;

            var description = transactionType == TransactionType.Purchase
                ? $"Mua gói {newSubscription.Name}"
                : $"Nâng cấp lên gói {newSubscription.Name}";

            var transaction = new Transaction
            {
                OrderCode = orderCode,
                UserId = userId,
                SubscriptionId = subscriptionId,
                Amount = newSubscription.Price,
                Status = TransactionStatus.Pending,
                Type = transactionType,
                PaymentProvider = PaymentType.Payos,
                Description = description
            };

            await _uow.Transactions.AddAsync(transaction, cancellationToken);

            var listItems = new List<ItemData>();

            var result = await _payOSService.CreatePaymentLinkAsync(
                orderCode,
                (int)newSubscription.Price,
                description,
                listItems,
                "https://success.com",
                "https://cancel.com"
            );

            // Store checkout URL returned by PayOS
            transaction.CheckoutUrl = result.checkoutUrl;
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("PayOS payment link created: OrderCode={OrderCode}", orderCode);

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

                    // Store the PayOS provider transaction reference
                    transaction.Status = TransactionStatus.Completed;
                    transaction.ProviderTransactionId = data.reference;
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

                    _logger.LogInformation(
                        "PayOS payment completed: OrderCode={OrderCode}, UserId={UserId}, Type={Type}, ProviderRef={Ref}",
                        data.orderCode, transaction.UserId, transaction.Type, data.reference);

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
            var transaction = await _uow.Transactions.GetByOrderCodeAsync(orderCode, cancellationToken)
                ?? throw new NotFoundException("Giao dịch không tồn tại.");

            _logger.LogInformation(
                "Cancelling {Provider} order: OrderCode={OrderCode}, Reason={Reason}",
                transaction.PaymentProvider, orderCode, reason);

            // For PayOS: call the PayOS cancel API so the payment link is invalidated on PayOS side.
            // For Momo: no cancel API exists — update DB only (Momo auto-expires the link).
            if (transaction.PaymentProvider == PaymentType.Payos)
            {
                var payosResult = await _payOSService.CancelPaymentLink(orderCode, reason, cancellationToken);
                if (payosResult == null)
                    throw new BusinessRuleException("Hủy thanh toán PayOS không thành công.");
            }

            transaction.Status = TransactionStatus.Cancelled;
            _uow.Transactions.Update(transaction);
            await _uow.SaveChangesAsync(cancellationToken);

            return new PaymentInformationResponse
            {
                OrderCode = transaction.OrderCode,
                Amount = (int)transaction.Amount,
                Status = TransactionStatus.Cancelled.ToString(),
                CancellationReason = reason,
                CreatedAt = transaction.CreatedAt.UtcDateTime
            };
        }

        // Momo payment flow

        public async Task<MomoPaymentLinkResponse> CreateMomoVipPaymentAsync(
            Guid userId,
            int subscriptionId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Creating Momo VIP payment for UserId={UserId}, SubscriptionId={SubscriptionId}",
                userId, subscriptionId);

            var newSubscription = await _uow.Subscriptions.GetByIdAsync(subscriptionId, cancellationToken)
                ?? throw new NotFoundException("Gói dịch vụ không tồn tại");

            // Block downgrade attempts
            var currentHighestSub = await _uow.UserSubscriptions.GetActiveSubscriptionAsync(userId, cancellationToken);
            var currentLevelOrder = currentHighestSub?.Subscription.LevelOrder ?? 0;

            if (newSubscription.LevelOrder < currentLevelOrder)
            {
                _logger.LogWarning(
                    "Momo downgrade attempt blocked: UserId={UserId}, CurrentLevel={CurrentLevel}, RequestedLevel={RequestedLevel}",
                    userId, currentLevelOrder, newSubscription.LevelOrder);
                throw new BusinessRuleException("Không thể hạ cấp gói dịch vụ. Vui lòng chọn gói cao hơn hoặc bằng gói hiện tại.");
            }

            var orderId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var momoTransactionType = currentHighestSub == null || newSubscription.Id == currentHighestSub.SubscriptionId
                ? TransactionType.Purchase
                : TransactionType.Upgrade;

            var description = momoTransactionType == TransactionType.Purchase
                ? $"Mua gói {newSubscription.Name}"
                : $"Nâng cấp lên gói {newSubscription.Name}";

            var transaction = new Transaction
            {
                OrderCode = long.Parse(orderId),
                UserId = userId,
                SubscriptionId = subscriptionId,
                Amount = newSubscription.Price,
                Status = TransactionStatus.Pending,
                Type = momoTransactionType,
                PaymentProvider = PaymentType.Momo,
                Description = description
            };

            await _uow.Transactions.AddAsync(transaction, cancellationToken);

            var momoResponse = await _momoService.CreatePaymentAsync(
                orderId,
                (long)newSubscription.Price,
                description,
                cancellationToken);

            if (momoResponse.ErrorCode != 0)
            {
                _logger.LogError(
                    "Momo API error: ErrorCode={ErrorCode}, Message={Message}",
                    momoResponse.ErrorCode, momoResponse.Message);
                throw new BusinessRuleException($"Tạo thanh toán Momo thất bại: {momoResponse.LocalMessage}");
            }

            // Store checkout URL returned by Momo
            transaction.CheckoutUrl = momoResponse.PayUrl;
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Momo payment link created: OrderId={OrderId}", orderId);

            return new MomoPaymentLinkResponse
            {
                PayUrl = momoResponse.PayUrl,
                OrderId = orderId
            };
        }

        public async Task<WebhookUpdateResult> ProcessMomoIpnAsync(
            MomoIpnRequest ipnRequest,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Processing Momo IPN: OrderId={OrderId}", ipnRequest.OrderId);

            if (!_momoService.VerifyIpnSignature(ipnRequest))
            {
                _logger.LogWarning("Momo IPN signature verification failed for OrderId={OrderId}", ipnRequest.OrderId);
                return new WebhookUpdateResult { IsSuccess = false, Message = "Chữ ký không hợp lệ" };
            }

            if (!long.TryParse(ipnRequest.OrderId, out var orderCode))
            {
                _logger.LogWarning("Momo IPN: Cannot parse OrderId as long: {OrderId}", ipnRequest.OrderId);
                return new WebhookUpdateResult { IsSuccess = false, Message = "OrderId không hợp lệ" };
            }

            bool isSuccess = ipnRequest.ErrorCode == "0";
            return await ActivateMomoSubscriptionAsync(
                orderCode,
                transId: ipnRequest.TransId,
                isSuccess,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<WebhookUpdateResult> SyncMomoPaymentAsync(
            long orderCode,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Syncing Momo payment status: OrderCode={OrderCode}", orderCode);

            var queryResult = await _momoService.QueryTransactionAsync(orderCode.ToString(), cancellationToken);

            _logger.LogInformation(
                "Momo query result: OrderCode={OrderCode}, ResultCode={ResultCode}, TransId={TransId}",
                orderCode, queryResult.ResultCode, queryResult.TransId);

            if (queryResult.ResultCode == 0)
            {
                return await ActivateMomoSubscriptionAsync(
                    orderCode,
                    transId: queryResult.TransId.ToString(),
                    isSuccess: true,
                    cancellationToken);
            }

            // Any non-zero code means payment is NOT confirmed as complete.
            // Do NOT modify DB — the transaction stays Pending so the user can retry or wait for IPN.
            _logger.LogWarning(
                "Momo sync: payment not confirmed for OrderCode={OrderCode}, ResultCode={ResultCode}, Message={Message}",
                orderCode, queryResult.ResultCode, queryResult.Message);

            return new WebhookUpdateResult
            {
                IsSuccess = false,
                Message = $"Thanh toán chưa hoàn tất: {queryResult.Message}"
            };
        }

        /// <summary>
        /// Shared activation logic for both IPN and sync-query flows.
        /// If <paramref name="isSuccess"/> is true, activates the subscription; otherwise marks the transaction as Cancelled.
        /// </summary>
        private async Task<WebhookUpdateResult> ActivateMomoSubscriptionAsync(
            long orderCode,
            string transId,
            bool isSuccess,
            CancellationToken cancellationToken)
        {
            var transaction = await _uow.Transactions.GetByOrderCodeAsync(orderCode, cancellationToken);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found for Momo OrderCode={OrderCode}", orderCode);
                return new WebhookUpdateResult { IsSuccess = false, Message = "Không tìm thấy đơn hàng" };
            }

            if (transaction.Status == TransactionStatus.Completed)
            {
                _logger.LogInformation("Momo transaction already completed: OrderCode={OrderCode}", orderCode);
                return new WebhookUpdateResult { IsSuccess = true, OrderCode = orderCode };
            }

            if (isSuccess)
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

                        _logger.LogInformation(
                            "Momo Case 1 - New purchase: UserId={UserId}, SubscriptionId={SubscriptionId}",
                            transaction.UserId, transaction.SubscriptionId);
                    }
                    else if (newSubscription.Id == currentHighestSub.SubscriptionId)
                    {
                        var maxEndDate = await _uow.UserSubscriptions.GetMaxEndDateBySubscriptionIdAsync(
                            transaction.UserId, newSubscription.Id, cancellationToken);

                        startDate = maxEndDate ?? DateTimeOffset.UtcNow;
                        endDate = startDate.AddDays(durationDays);

                        _logger.LogInformation(
                            "Momo Case 2 - Stacking: UserId={UserId}, SubscriptionId={SubscriptionId}, StartDate={StartDate}",
                            transaction.UserId, transaction.SubscriptionId, startDate);
                    }
                    else if (newSubscription.LevelOrder > currentHighestSub.Subscription.LevelOrder)
                    {
                        startDate = DateTimeOffset.UtcNow;
                        endDate = startDate.AddDays(durationDays);

                        await _uow.UserSubscriptions.MarkAllActiveAsUpgradedAsync(transaction.UserId, cancellationToken);

                        _logger.LogInformation(
                            "Momo Case 3 - Upgrade: UserId={UserId}, OldLevel={OldLevel}, NewLevel={NewLevel}",
                            transaction.UserId, currentHighestSub.Subscription.LevelOrder, newSubscription.LevelOrder);
                    }
                    else
                    {
                        _logger.LogError(
                            "Momo: Downgrade attempt detected for UserId={UserId}",
                            transaction.UserId);
                        throw new BusinessRuleException("Không thể hạ cấp gói dịch vụ.");
                    }

                    transaction.Status = TransactionStatus.Completed;
                    transaction.ProviderTransactionId = transId;
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

                    _logger.LogInformation(
                        "Momo payment completed: OrderCode={OrderCode}, UserId={UserId}, TransId={TransId}",
                        orderCode, transaction.UserId, transId);

                    return new WebhookUpdateResult { IsSuccess = true, OrderCode = orderCode };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error activating Momo subscription for OrderCode={OrderCode}", orderCode);
                    await _uow.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            }

            // Non-zero error code means payment failed / not yet paid
            transaction.Status = TransactionStatus.Cancelled;
            _uow.Transactions.Update(transaction);
            await _uow.SaveChangesAsync(cancellationToken);

            return new WebhookUpdateResult { IsSuccess = false, Message = "Thanh toán Momo thất bại" };
        }
    }
}
