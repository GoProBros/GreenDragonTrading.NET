using GreenDragonTrading.Application.DTOs;
using Net.payOS.Types;

namespace GreenDragonTrading.Application.Interfaces
{
    public interface IPaymentService
    {
        //PayOS 
        Task<PaymentLinkResponse> CreateVipPaymentAsync(Guid userId, int subscriptionId, CancellationToken cancellationToken = default);
        Task<WebhookUpdateResult> ProcessWebhookAsync(WebhookType webhookBody, CancellationToken cancellationToken = default);
        Task<PaymentStatusResponse?> GetPaymentStatusAsync(long orderCode, Guid userId, CancellationToken cancellationToken = default);
        Task<PaymentInformationResponse> CancelPaymentAsync(long orderCode, string reason, CancellationToken cancellationToken = default);

        //Momo 
        Task<MomoPaymentLinkResponse> CreateMomoVipPaymentAsync(Guid userId, int subscriptionId, CancellationToken cancellationToken = default);
        Task<WebhookUpdateResult> ProcessMomoIpnAsync(MomoIpnRequest ipnRequest, CancellationToken cancellationToken = default);
        Task<WebhookUpdateResult> SyncMomoPaymentAsync(long orderCode, CancellationToken cancellationToken = default);
    }
}
