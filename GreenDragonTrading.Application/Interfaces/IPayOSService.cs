using Net.payOS.Types;

namespace GreenDragonTrading.Application.Interfaces
{
    public interface IPayOSService
    {
        Task<CreatePaymentResult> CreatePaymentLinkAsync(long orderCode, int amount, string description, List<ItemData> listItems, string returnUrl, string cancelUrl, CancellationToken cancellationToken = default);
        WebhookData VerifyWebhook(WebhookType webhookBody, CancellationToken cancellationToken = default);
        Task<PaymentLinkInformation> CancelPaymentLink(long orderCode, string reason, CancellationToken cancellationToken = default);
    }
}