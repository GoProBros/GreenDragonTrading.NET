using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.ProcessPayOSWebhook
{
    public class ProcessPayOSWebhookCommandHandler : IRequestHandler<ProcessPayOSWebhookCommand, ApiResponse>
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<ProcessPayOSWebhookCommandHandler> _logger;

        public ProcessPayOSWebhookCommandHandler(
            IPaymentService paymentService,
            ILogger<ProcessPayOSWebhookCommandHandler> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(
            ProcessPayOSWebhookCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing PayOS webhook");

            await _paymentService.ProcessWebhookAsync(request.WebhookBody, cancellationToken);

            return ApiResponse.Success("Webhook processed successfully.");
        }
    }
}
