using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Net.payOS.Types;
using Transaction = GreenDragonTrading.Domain.Entities.Transaction;

namespace GreenDragonTrading.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPayOSService _payOSService;
        private readonly IMomoService _momoService;
        private readonly IRedisService _redisService;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<PaymentService> _logger;
        private readonly PayOSOptions _payOSOptions;
        private readonly MomoOptions _momoOptions;

        public PaymentService(
            IPayOSService payOSService,
            IMomoService momoService,
            IRedisService redisService,
            IUnitOfWork uow,
            ILogger<PaymentService> logger,
            IOptions<PayOSOptions> payOSOptions,
            IOptions<MomoOptions> momoOptions)
        {
            _payOSService = payOSService;
            _momoService = momoService;
            _redisService = redisService;
            _uow = uow;
            _logger = logger;
            _payOSOptions = payOSOptions.Value;
            _momoOptions = momoOptions.Value;
        }

        public async Task<PaymentLinkResponse> CreateVipPaymentAsync(Guid userId, int subscriptionId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating VIP payment for UserId={UserId}, SubscriptionId={SubscriptionId}", userId, subscriptionId);

            var newSubscription = await _uow.Subscriptions.GetByIdAsync(subscriptionId, cancellationToken)
                      ?? throw new NotFoundException("Gói dịch vụ không tồn tại");

            await EnsureSubscriptionCanBePurchasedAsync(userId, newSubscription, cancellationToken);

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

            var gatewayDescription = BuildGatewayDescription(transactionType, newSubscription.Id);

            var purchasedAt = DateTimeOffset.UtcNow;
            var transactionDescription = BuildTransactionDescription(gatewayDescription, newSubscription.Price, purchasedAt);

            var transaction = new Transaction
            {
                OrderCode = orderCode,
                UserId = userId,
                SubscriptionId = subscriptionId,
                Amount = newSubscription.Price,
                Status = TransactionStatus.Pending,
                Type = transactionType,
                PaymentProvider = PaymentType.Payos,
                Description = transactionDescription,
                CreatedAt = purchasedAt
            };

            await _uow.Transactions.AddAsync(transaction, cancellationToken);

            var listItems = new List<ItemData>();
            var payOSExpiredAt = BuildPayOSExpiredAtUnix(purchasedAt, _payOSOptions.ExpirationMinutes);

            var result = await _payOSService.CreatePaymentLinkAsync(
                orderCode,
                (int)newSubscription.Price,
                gatewayDescription,
                listItems,
                _payOSOptions.ReturnUrl,
                _payOSOptions.CancelUrl,
                payOSExpiredAt,
                cancellationToken
            );

            transaction.CheckoutUrl = result.checkoutUrl;
            await _uow.SaveChangesAsync(cancellationToken);
            await TrackPendingPaymentForSyncAsync(orderCode, payOSExpiredAt);

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
                await RemovePendingPaymentFromSyncQueueAsync(data.orderCode);
                return new WebhookUpdateResult { IsSuccess = false, Message = "Không tìm thấy đơn hàng" };
            }

            if (transaction.Status == TransactionStatus.Completed)
            {
                _logger.LogInformation("Transaction already completed: OrderCode={OrderCode}", data.orderCode);
                await RemovePendingPaymentFromSyncQueueAsync(data.orderCode);
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
                    else if (newSubscription.LevelOrder == currentHighestSub.Subscription.LevelOrder
                        && newSubscription.Id != currentHighestSub.SubscriptionId)
                    {
                        _logger.LogWarning(
                            "Same level purchase blocked: UserId={UserId}, CurrentSubscriptionId={CurrentSubscriptionId}, RequestedSubscriptionId={RequestedSubscriptionId}",
                            transaction.UserId,
                            currentHighestSub.SubscriptionId,
                            newSubscription.Id);
                        throw new BusinessRuleException(
                            "Bạn đang sử dụng gói cùng cấp. Vui lòng dùng hết gói hiện tại trước khi đăng ký gói mới hoặc liên hệ CSKH để hủy gói hiện tại.");
                    }
                    else
                    {
                        _logger.LogError("Downgrade attempt in webhook - this should have been blocked: UserId={UserId}", transaction.UserId);
                        throw new BusinessRuleException("Không thể hạ cấp gói dịch vụ.");
                    }

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
                    await RemovePendingPaymentFromSyncQueueAsync(data.orderCode);

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
                PaymentProvider = transaction.PaymentProvider,
                PaymentProviderName = transaction.PaymentProvider.GetDisplayName(),
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

            if (transaction.PaymentProvider == PaymentType.Payos)
            {
                var payosResult = await _payOSService.CancelPaymentLink(orderCode, reason, cancellationToken);
                if (payosResult == null)
                    throw new BusinessRuleException("Hủy thanh toán PayOS không thành công.");
            }

            transaction.Status = TransactionStatus.Cancelled;
            _uow.Transactions.Update(transaction);
            await _uow.SaveChangesAsync(cancellationToken);
            await RemovePendingPaymentFromSyncQueueAsync(orderCode);

            return new PaymentInformationResponse
            {
                OrderCode = transaction.OrderCode,
                Amount = (int)transaction.Amount,
                Status = TransactionStatus.Cancelled.ToString(),
                CancellationReason = reason,
                CreatedAt = transaction.CreatedAt.UtcDateTime
            };
        }

        public async Task<WebhookUpdateResult> SyncPaymentAsync(long orderCode, CancellationToken cancellationToken = default)
        {
            var transaction = await _uow.Transactions.GetByOrderCodeAsync(orderCode, cancellationToken);
            if (transaction == null)
            {
                _logger.LogWarning("Sync payment: transaction not found for OrderCode={OrderCode}", orderCode);
                await RemovePendingPaymentFromSyncQueueAsync(orderCode);
                return new WebhookUpdateResult
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy đơn hàng"
                };
            }

            if (transaction.PaymentProvider == PaymentType.Payos)
            {
                return await SyncPayOSPaymentAsync(transaction, cancellationToken);
            }

            if (transaction.PaymentProvider == PaymentType.Momo)
            {
                return await SyncMomoPaymentAsync(orderCode, cancellationToken);
            }

            _logger.LogWarning(
                "Sync payment: unsupported provider {Provider} for OrderCode={OrderCode}",
                transaction.PaymentProvider,
                orderCode);

            return new WebhookUpdateResult
            {
                IsSuccess = false,
                Message = "Nhà cung cấp thanh toán không được hỗ trợ"
            };
        }

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

            await EnsureSubscriptionCanBePurchasedAsync(userId, newSubscription, cancellationToken);

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

            var gatewayDescription = BuildGatewayDescription(momoTransactionType, newSubscription.Id);

            var purchasedAt = DateTimeOffset.UtcNow;
            var transactionDescription = BuildTransactionDescription(gatewayDescription, newSubscription.Price, purchasedAt);

            var transaction = new Transaction
            {
                OrderCode = long.Parse(orderId),
                UserId = userId,
                SubscriptionId = subscriptionId,
                Amount = newSubscription.Price,
                Status = TransactionStatus.Pending,
                Type = momoTransactionType,
                PaymentProvider = PaymentType.Momo,
                Description = transactionDescription,
                CreatedAt = purchasedAt
            };

            await _uow.Transactions.AddAsync(transaction, cancellationToken);

            var momoResponse = await _momoService.CreatePaymentAsync(
                orderId,
                (long)newSubscription.Price,
                gatewayDescription,
                cancellationToken);

            if (momoResponse.ErrorCode != 0)
            {
                _logger.LogError(
                    "Momo API error: ErrorCode={ErrorCode}, Message={Message}",
                    momoResponse.ErrorCode, momoResponse.Message);
                throw new BusinessRuleException($"Tạo thanh toán Momo thất bại: {momoResponse.LocalMessage}");
            }

            transaction.CheckoutUrl = momoResponse.PayUrl;
            await _uow.SaveChangesAsync(cancellationToken);

            var momoExpiredAt = BuildMomoExpiredAtUnix(purchasedAt, _momoOptions.ExpirationMinutes);
            await TrackPendingPaymentForSyncAsync(transaction.OrderCode, momoExpiredAt);

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

        public async Task<WebhookUpdateResult> SyncMomoPaymentAsync(
            long orderCode,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Syncing Momo payment status: OrderCode={OrderCode}", orderCode);

            var transaction = await _uow.Transactions.GetByOrderCodeAsync(orderCode, cancellationToken);
            if (transaction == null)
            {
                _logger.LogWarning("Momo sync: transaction not found for OrderCode={OrderCode}", orderCode);
                await RemovePendingPaymentFromSyncQueueAsync(orderCode);
                return new WebhookUpdateResult
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy đơn hàng"
                };
            }

            if (transaction.PaymentProvider != PaymentType.Momo)
            {
                _logger.LogWarning(
                    "Momo sync called for non-Momo transaction: OrderCode={OrderCode}, Provider={Provider}",
                    orderCode,
                    transaction.PaymentProvider);

                return new WebhookUpdateResult
                {
                    IsSuccess = false,
                    Message = "Đơn hàng này không sử dụng Momo"
                };
            }

            if (transaction.Status == TransactionStatus.Completed)
            {
                await RemovePendingPaymentFromSyncQueueAsync(orderCode);
                return new WebhookUpdateResult
                {
                    IsSuccess = true,
                    OrderCode = orderCode,
                    Message = "Giao dịch đã hoàn tất"
                };
            }

            if (transaction.Status == TransactionStatus.Cancelled || transaction.Status == TransactionStatus.Expired)
            {
                await RemovePendingPaymentFromSyncQueueAsync(orderCode);
                return new WebhookUpdateResult
                {
                    IsSuccess = false,
                    OrderCode = orderCode,
                    Message = $"Giao dịch đã ở trạng thái {transaction.Status.GetDisplayName()}"
                };
            }

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

            if (HasTimedOut(transaction.CreatedAt, _momoOptions.ExpirationMinutes))
            {
                transaction.Status = TransactionStatus.Expired;
                _uow.Transactions.Update(transaction);
                await _uow.SaveChangesAsync(cancellationToken);
                await RemovePendingPaymentFromSyncQueueAsync(orderCode);

                _logger.LogWarning(
                    "Momo sync marked transaction as Expired due timeout: OrderCode={OrderCode}, ResultCode={ResultCode}",
                    orderCode,
                    queryResult.ResultCode);

                return new WebhookUpdateResult
                {
                    IsSuccess = false,
                    OrderCode = orderCode,
                    Message = "Giao dịch đã hết hạn và được cập nhật trong hệ thống"
                };
            }

            _logger.LogWarning(
                "Momo sync: payment not confirmed for OrderCode={OrderCode}, ResultCode={ResultCode}, Message={Message}",
                orderCode, queryResult.ResultCode, queryResult.Message);

            return new WebhookUpdateResult
            {
                IsSuccess = false,
                Message = $"Thanh toán chưa hoàn tất: {queryResult.Message}"
            };
        }

        private async Task<WebhookUpdateResult> SyncPayOSPaymentAsync(
            Transaction transaction,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Syncing PayOS payment status: OrderCode={OrderCode}", transaction.OrderCode);

            if (transaction.Status == TransactionStatus.Completed)
            {
                await RemovePendingPaymentFromSyncQueueAsync(transaction.OrderCode);
                return new WebhookUpdateResult
                {
                    IsSuccess = true,
                    OrderCode = transaction.OrderCode,
                    Message = "Giao dịch đã hoàn tất"
                };
            }

            if (transaction.Status == TransactionStatus.Cancelled || transaction.Status == TransactionStatus.Expired)
            {
                await RemovePendingPaymentFromSyncQueueAsync(transaction.OrderCode);
                return new WebhookUpdateResult
                {
                    IsSuccess = false,
                    OrderCode = transaction.OrderCode,
                    Message = $"Giao dịch đã ở trạng thái {transaction.Status.GetDisplayName()}"
                };
            }

            PaymentLinkInformation payOSInfo;
            try
            {
                payOSInfo = await _payOSService.GetPaymentLinkInformation(transaction.OrderCode, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to query PayOS payment information: OrderCode={OrderCode}", transaction.OrderCode);
                return new WebhookUpdateResult
                {
                    IsSuccess = false,
                    Message = "Không thể đồng bộ trạng thái PayOS lúc này"
                };
            }

            var payOSStatus = (payOSInfo.status ?? string.Empty).Trim().ToUpperInvariant();

            _logger.LogInformation(
                "PayOS sync result: OrderCode={OrderCode}, Status={Status}, AmountPaid={AmountPaid}, AmountRemaining={AmountRemaining}",
                transaction.OrderCode,
                payOSStatus,
                payOSInfo.amountPaid,
                payOSInfo.amountRemaining);

            if (payOSStatus == "PAID" || payOSStatus == "SUCCESS")
            {
                var providerReference = string.IsNullOrWhiteSpace(payOSInfo.id)
                    ? transaction.OrderCode.ToString()
                    : payOSInfo.id;

                return await ActivatePayOSSubscriptionAsync(
                    transaction.OrderCode,
                    providerReference,
                    cancellationToken);
            }

            if (payOSStatus == "EXPIRED" || HasTimedOut(transaction.CreatedAt, _payOSOptions.ExpirationMinutes))
            {
                transaction.Status = TransactionStatus.Expired;
                _uow.Transactions.Update(transaction);
                await _uow.SaveChangesAsync(cancellationToken);
                await RemovePendingPaymentFromSyncQueueAsync(transaction.OrderCode);

                _logger.LogWarning(
                    "PayOS sync marked transaction as Expired: OrderCode={OrderCode}, PayOSStatus={Status}",
                    transaction.OrderCode,
                    payOSStatus);

                return new WebhookUpdateResult
                {
                    IsSuccess = false,
                    OrderCode = transaction.OrderCode,
                    Message = "Giao dịch đã hết hạn và được cập nhật trong hệ thống"
                };
            }

            if (payOSStatus == "CANCELLED" || payOSStatus == "CANCELED")
            {
                transaction.Status = TransactionStatus.Cancelled;
                _uow.Transactions.Update(transaction);
                await _uow.SaveChangesAsync(cancellationToken);
                await RemovePendingPaymentFromSyncQueueAsync(transaction.OrderCode);

                _logger.LogWarning(
                    "PayOS sync marked transaction as Cancelled: OrderCode={OrderCode}",
                    transaction.OrderCode);

                return new WebhookUpdateResult
                {
                    IsSuccess = false,
                    OrderCode = transaction.OrderCode,
                    Message = "Giao dịch đã bị hủy và được cập nhật trong hệ thống"
                };
            }

            return new WebhookUpdateResult
            {
                IsSuccess = false,
                OrderCode = transaction.OrderCode,
                Message = "Thanh toán chưa hoàn tất"
            };
        }

        private async Task<WebhookUpdateResult> ActivatePayOSSubscriptionAsync(
            long orderCode,
            string providerReference,
            CancellationToken cancellationToken)
        {
            var transaction = await _uow.Transactions.GetByOrderCodeAsync(orderCode, cancellationToken);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found for PayOS OrderCode={OrderCode}", orderCode);
                await RemovePendingPaymentFromSyncQueueAsync(orderCode);
                return new WebhookUpdateResult { IsSuccess = false, Message = "Không tìm thấy đơn hàng" };
            }

            if (transaction.Status == TransactionStatus.Completed)
            {
                _logger.LogInformation("PayOS transaction already completed: OrderCode={OrderCode}", orderCode);
                await RemovePendingPaymentFromSyncQueueAsync(orderCode);
                return new WebhookUpdateResult { IsSuccess = true, OrderCode = orderCode };
            }

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
                        "PayOS sync case 1 - New purchase: UserId={UserId}, SubscriptionId={SubscriptionId}",
                        transaction.UserId,
                        transaction.SubscriptionId);
                }
                else if (newSubscription.Id == currentHighestSub.SubscriptionId)
                {
                    var maxEndDate = await _uow.UserSubscriptions.GetMaxEndDateBySubscriptionIdAsync(
                        transaction.UserId,
                        newSubscription.Id,
                        cancellationToken);

                    startDate = maxEndDate ?? DateTimeOffset.UtcNow;
                    endDate = startDate.AddDays(durationDays);

                    _logger.LogInformation(
                        "PayOS sync case 2 - Stacking: UserId={UserId}, SubscriptionId={SubscriptionId}, StartDate={StartDate}",
                        transaction.UserId,
                        transaction.SubscriptionId,
                        startDate);
                }
                else if (newSubscription.LevelOrder > currentHighestSub.Subscription.LevelOrder)
                {
                    startDate = DateTimeOffset.UtcNow;
                    endDate = startDate.AddDays(durationDays);

                    await _uow.UserSubscriptions.MarkAllActiveAsUpgradedAsync(transaction.UserId, cancellationToken);

                    _logger.LogInformation(
                        "PayOS sync case 3 - Upgrade: UserId={UserId}, OldLevel={OldLevel}, NewLevel={NewLevel}",
                        transaction.UserId,
                        currentHighestSub.Subscription.LevelOrder,
                        newSubscription.LevelOrder);
                }
                else if (newSubscription.LevelOrder == currentHighestSub.Subscription.LevelOrder
                    && newSubscription.Id != currentHighestSub.SubscriptionId)
                {
                    _logger.LogWarning(
                        "PayOS sync same level purchase blocked: UserId={UserId}, CurrentSubscriptionId={CurrentSubscriptionId}, RequestedSubscriptionId={RequestedSubscriptionId}",
                        transaction.UserId,
                        currentHighestSub.SubscriptionId,
                        newSubscription.Id);
                    throw new BusinessRuleException(
                        "Bạn đang sử dụng gói cùng cấp. Vui lòng dùng hết gói hiện tại trước khi đăng ký gói mới hoặc liên hệ CSKH để hủy gói hiện tại.");
                }
                else
                {
                    _logger.LogError("PayOS sync downgrade attempt detected: UserId={UserId}", transaction.UserId);
                    throw new BusinessRuleException("Không thể hạ cấp gói dịch vụ.");
                }

                transaction.Status = TransactionStatus.Completed;
                transaction.ProviderTransactionId = providerReference;
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
                await RemovePendingPaymentFromSyncQueueAsync(orderCode);

                _logger.LogInformation(
                    "PayOS payment completed by sync: OrderCode={OrderCode}, UserId={UserId}, ProviderRef={Ref}",
                    transaction.OrderCode,
                    transaction.UserId,
                    providerReference);

                return new WebhookUpdateResult { IsSuccess = true, OrderCode = transaction.OrderCode };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating PayOS subscription for OrderCode={OrderCode}", orderCode);
                await _uow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        private static bool HasTimedOut(DateTimeOffset createdAt, int expirationMinutes)
        {
            var validExpirationMinutes = expirationMinutes > 0 ? expirationMinutes : 1;
            return DateTimeOffset.UtcNow - createdAt >= TimeSpan.FromMinutes(validExpirationMinutes);
        }

        private static long BuildPayOSExpiredAtUnix(DateTimeOffset createdAt, int expirationMinutes)
        {
            var validExpirationMinutes = expirationMinutes > 0 ? expirationMinutes : 30;
            return createdAt.AddMinutes(validExpirationMinutes).ToUnixTimeSeconds();
        }

        private static long BuildMomoExpiredAtUnix(DateTimeOffset createdAt, int expirationMinutes)
        {
            var validExpirationMinutes = expirationMinutes > 0 ? expirationMinutes : 15;
            return createdAt.AddMinutes(validExpirationMinutes).ToUnixTimeSeconds();
        }

        private async Task TrackPendingPaymentForSyncAsync(long orderCode, long expiredAtUnix)
        {
            try
            {
                await _redisService.SortedSetAddAsync(
                    RedisConstants.PendingPaymentSyncQueue(),
                    orderCode.ToString(),
                    expiredAtUnix);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to track pending payment in Redis queue: OrderCode={OrderCode}, ExpiredAtUnix={ExpiredAtUnix}",
                    orderCode,
                    expiredAtUnix);
            }
        }

        private async Task RemovePendingPaymentFromSyncQueueAsync(long orderCode)
        {
            try
            {
                await _redisService.SortedSetRemoveAsync(
                    RedisConstants.PendingPaymentSyncQueue(),
                    orderCode.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to remove pending payment from Redis queue: OrderCode={OrderCode}",
                    orderCode);
            }
        }

        private async Task EnsureSubscriptionCanBePurchasedAsync(
            Guid userId,
            Subscription subscription,
            CancellationToken cancellationToken)
        {
            if (subscription.IsActive == CommonStatus.Active)
            {
                return;
            }

            var user = await _uow.Users.GetByIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException("Người dùng không tồn tại");

            var isAdminOrStaff = user.Role == UserRole.Admin || user.Role == UserRole.Staff;
            if (isAdminOrStaff)
            {
                return;
            }

            _logger.LogWarning(
                "Hidden subscription purchase blocked: UserId={UserId}, SubscriptionId={SubscriptionId}",
                userId,
                subscription.Id);

            throw new BusinessRuleException("Gói dịch vụ này đã được ẩn và không thể đăng ký mới.");
        }

        private static string BuildGatewayDescription(TransactionType transactionType, int subscriptionId)
        {
            var description = transactionType == TransactionType.Purchase
                ? $"Mua goi {subscriptionId}"
                : $"Nang cap goi {subscriptionId}";

            return description.Length <= 25
                ? description
                : description[..25];
        }

        private static string BuildTransactionDescription(
            string gatewayDescription,
            decimal purchasedPrice,
            DateTimeOffset purchasedAt)
        {
            var vietnamTime = purchasedAt.ToOffset(TimeSpan.FromHours(7));
            var description = $"{gatewayDescription} | Gia luc mua: {purchasedPrice:0.##} VND | Ngay mua: {vietnamTime:dd/MM/yyyy HH:mm:ss}";

            return description.Length <= 255
                ? description
                : description[..255];
        }

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
                await RemovePendingPaymentFromSyncQueueAsync(orderCode);
                return new WebhookUpdateResult { IsSuccess = false, Message = "Không tìm thấy đơn hàng" };
            }

            if (transaction.Status == TransactionStatus.Completed)
            {
                _logger.LogInformation("Momo transaction already completed: OrderCode={OrderCode}", orderCode);
                await RemovePendingPaymentFromSyncQueueAsync(orderCode);
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
                    else if (newSubscription.LevelOrder == currentHighestSub.Subscription.LevelOrder
                        && newSubscription.Id != currentHighestSub.SubscriptionId)
                    {
                        _logger.LogWarning(
                            "Momo same level purchase blocked: UserId={UserId}, CurrentSubscriptionId={CurrentSubscriptionId}, RequestedSubscriptionId={RequestedSubscriptionId}",
                            transaction.UserId,
                            currentHighestSub.SubscriptionId,
                            newSubscription.Id);
                        throw new BusinessRuleException(
                            "Bạn đang sử dụng gói cùng cấp. Vui lòng dùng hết gói hiện tại trước khi đăng ký gói mới hoặc liên hệ CSKH để hủy gói hiện tại.");
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
                    await RemovePendingPaymentFromSyncQueueAsync(orderCode);

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

            transaction.Status = TransactionStatus.Cancelled;
            _uow.Transactions.Update(transaction);
            await _uow.SaveChangesAsync(cancellationToken);
            await RemovePendingPaymentFromSyncQueueAsync(orderCode);

            return new WebhookUpdateResult { IsSuccess = false, Message = "Thanh toán Momo thất bại" };
        }
    }
}
