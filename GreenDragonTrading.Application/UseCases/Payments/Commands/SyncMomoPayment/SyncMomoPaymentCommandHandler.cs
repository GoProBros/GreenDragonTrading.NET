using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.SyncMomoPayment
{
    /// <summary>
    /// Handles <see cref="SyncMomoPaymentCommand"/>.
    /// Syncs payment status with the configured payment provider and updates local DB state.
    /// </summary>
    public class SyncMomoPaymentCommandHandler
        : IRequestHandler<SyncMomoPaymentCommand, ApiResponse<WebhookUpdateResult>>
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<SyncMomoPaymentCommandHandler> _logger;

        public SyncMomoPaymentCommandHandler(
            IPaymentService paymentService,
            ILogger<SyncMomoPaymentCommandHandler> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        public async Task<ApiResponse<WebhookUpdateResult>> Handle(
            SyncMomoPaymentCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Syncing payment for OrderCode={OrderCode}", request.OrderCode);

            var result = await _paymentService.SyncPaymentAsync(request.OrderCode, cancellationToken);

            if (result.IsSuccess)
            {
                return ApiResponse<WebhookUpdateResult>.Success(result, "Đồng bộ thanh toán thành công.");
            }

            return ApiResponse<WebhookUpdateResult>.Failure(result.Message);
        }
    }
}
