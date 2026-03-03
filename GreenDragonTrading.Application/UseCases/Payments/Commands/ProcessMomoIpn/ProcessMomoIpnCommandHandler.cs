using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.ProcessMomoIpn
{
    /// <summary>
    /// Handles <see cref="ProcessMomoIpnCommand"/>.
    /// </summary>
    public class ProcessMomoIpnCommandHandler : IRequestHandler<ProcessMomoIpnCommand, ApiResponse>
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<ProcessMomoIpnCommandHandler> _logger;

        public ProcessMomoIpnCommandHandler(
            IPaymentService paymentService,
            ILogger<ProcessMomoIpnCommandHandler> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(
            ProcessMomoIpnCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing Momo IPN for OrderId={OrderId}", request.IpnRequest.OrderId);

            await _paymentService.ProcessMomoIpnAsync(request.IpnRequest, cancellationToken);

            return ApiResponse.Success("Momo IPN processed successfully.");
        }
    }
}
