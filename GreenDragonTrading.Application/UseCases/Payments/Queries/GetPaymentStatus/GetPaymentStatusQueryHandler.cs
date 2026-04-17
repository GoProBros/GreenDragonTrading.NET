using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Payments.Queries.GetPaymentStatus
{
    public class GetPaymentStatusQueryHandler : IRequestHandler<GetPaymentStatusQuery, ApiResponse<PaymentStatusResponse>>
    {
        private readonly IPaymentService _paymentService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtService _jwtService;
        private readonly ILogger<GetPaymentStatusQueryHandler> _logger;

        public GetPaymentStatusQueryHandler(
            IPaymentService paymentService,
            IHttpContextAccessor httpContextAccessor,
            IJwtService jwtService,
            ILogger<GetPaymentStatusQueryHandler> logger)
        {
            _paymentService = paymentService;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ApiResponse<PaymentStatusResponse>> Handle(
            GetPaymentStatusQuery request,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new UnauthenticatedException("Không tìm thấy HTTP context.");
            }

            var authHeaderValue = httpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeaderValue) || !authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthenticatedException("Không tìm thấy Authorization header.");
            }

            var accessToken = authHeaderValue.Substring("Bearer ".Length).Trim();
            var tokenInfo = _jwtService.GetTokenInfo(accessToken);
            if (tokenInfo == null)
            {
                throw new UnauthenticatedException("Access token không hợp lệ.");
            }

            _logger.LogInformation("Getting payment status for OrderCode={OrderCode}, UserId={UserId}", 
                request.OrderCode, tokenInfo.UserId);

            var response = await _paymentService.GetPaymentStatusAsync(
                request.OrderCode,
                tokenInfo.UserId,
                cancellationToken);

            if (response == null)
            {
                throw new NotFoundException("Giao dịch không tồn tại.");
            }

            // If webhook/IPN is delayed or missed, automatically sync pending transactions
            // so frontend only needs to call the status endpoint.
            if (response.Status == TransactionStatus.Pending
                && DateTimeOffset.UtcNow - response.CreatedAt >= TimeSpan.FromSeconds(10))
            {
                _logger.LogInformation(
                    "Auto syncing pending payment for OrderCode={OrderCode}, Provider={Provider}",
                    request.OrderCode,
                    response.PaymentProvider);

                await _paymentService.SyncPaymentAsync(request.OrderCode, cancellationToken);

                response = await _paymentService.GetPaymentStatusAsync(
                    request.OrderCode,
                    tokenInfo.UserId,
                    cancellationToken);

                if (response == null)
                {
                    throw new NotFoundException("Giao dịch không tồn tại.");
                }
            }

            _logger.LogInformation("Payment status retrieved: OrderCode={OrderCode}, Status={Status}", 
                request.OrderCode, response.Status);

            return ApiResponse<PaymentStatusResponse>.Success(response, "Lấy trạng thái thanh toán thành công.");
        }
    }
}
