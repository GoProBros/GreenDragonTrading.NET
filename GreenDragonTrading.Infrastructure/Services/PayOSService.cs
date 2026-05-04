using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.Extensions.Options;
using Net.payOS;
using Net.payOS.Types;

namespace GreenDragonTrading.Infrastructure.Services
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOS _payOS;

        public PayOSService(IOptions<PayOSOptions> options)
        {
            var settings = options.Value;
            _payOS = new PayOS(settings.ClientId, settings.ApiKey, settings.ChecksumKey);
        }

        public async Task<CreatePaymentResult> CreatePaymentLinkAsync(long orderCode, int amount, string description, List<ItemData> listItems, string returnUrl, string cancelUrl, long? expiredAt = null, CancellationToken cancellationToken = default)
        {

            var paymentData = new PaymentData(
                orderCode,
                amount,
                description,
                listItems,
                cancelUrl,
                returnUrl
            )
            {
                expiredAt = expiredAt
            };

            return await _payOS.createPaymentLink(paymentData);
        }

        public WebhookData VerifyWebhook(WebhookType webhookBody, CancellationToken cancellationToken = default)
        {
            return _payOS.verifyPaymentWebhookData(webhookBody);
        }

        public async Task<PaymentLinkInformation> GetPaymentLinkInformation(long orderCode, CancellationToken cancellationToken = default)
        {
            return await _payOS.getPaymentLinkInformation(orderCode);
        }

        public async Task<PaymentLinkInformation> CancelPaymentLink(long orderCode, string reason, CancellationToken cancellationToken = default)
        {
            return await _payOS.cancelPaymentLink(orderCode, reason);
        }
    }
}