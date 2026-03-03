using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Low-level Momo payment gateway integration service.
    /// Handles direct communication with the Momo API.
    /// </summary>
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponse> CreatePaymentAsync(
            string orderId,
            long amount,
            string orderInfo,
            CancellationToken cancellationToken = default);
        bool VerifyIpnSignature(MomoIpnRequest ipnRequest);
        Task<MomoQueryTransactionResponse> QueryTransactionAsync(
            string orderId,
            CancellationToken cancellationToken = default);
    }
}
