

namespace GreenDragonTrading.Infrastructure.Services
{
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using GreenDragonTrading.Application.Common.Options;
    using GreenDragonTrading.Application.DTOs;
    using GreenDragonTrading.Application.Interfaces;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Low-level Momo payment gateway integration service.
    /// Handles direct communication with the Momo API using HMAC-SHA256 signatures.
    /// </summary>
    public class MomoService : IMomoService
    {
        private readonly MomoOptions _options;
        private readonly HttpClient _httpClient;
        private readonly ILogger<MomoService> _logger;

        public MomoService(
            IOptions<MomoOptions> options,
            HttpClient httpClient,
            ILogger<MomoService> logger)
        {
            _options = options.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<MomoCreatePaymentResponse> CreatePaymentAsync(
            string orderId,
            long amount,
            string orderInfo,
            CancellationToken cancellationToken = default)
        {
            var requestId = Guid.NewGuid().ToString();
            var extraData = string.Empty;

            // Build raw signature string (order matters for Momo legacy API)
            var rawHash =
                $"partnerCode={_options.PartnerCode}" +
                $"&accessKey={_options.AccessKey}" +
                $"&requestId={requestId}" +
                $"&amount={amount}" +
                $"&orderId={orderId}" +
                $"&orderInfo={orderInfo}" +
                $"&returnUrl={_options.ReturnUrl}" +
                $"&notifyUrl={_options.NotifyUrl}" +
                $"&extraData={extraData}";

            var signature = ComputeHmacSha256(rawHash, _options.SecretKey);

            var requestBody = new
            {
                partnerCode = _options.PartnerCode,
                accessKey = _options.AccessKey,
                requestId,
                amount = amount.ToString(),
                orderId,
                orderInfo,
                returnUrl = _options.ReturnUrl,
                notifyUrl = _options.NotifyUrl,
                extraData,
                requestType = _options.RequestType,
                signature
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation(
                "Sending Momo payment creation request: OrderId={OrderId}, Amount={Amount}",
                orderId, amount);

            var response = await _httpClient.PostAsync(_options.ApiEndpoint, jsonContent, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("Momo API response: {Response}", responseContent);

            var result = JsonSerializer.Deserialize<MomoCreatePaymentResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? throw new InvalidOperationException("Failed to deserialize Momo payment response.");

            return result;
        }

        /// <inheritdoc/>
        public bool VerifyIpnSignature(MomoIpnRequest ipnRequest)
        {
            // Raw signature string for IPN verification (order matters)
            var rawHash =
                $"partnerCode={ipnRequest.PartnerCode}" +
                $"&accessKey={ipnRequest.AccessKey}" +
                $"&requestId={ipnRequest.RequestId}" +
                $"&amount={ipnRequest.Amount}" +
                $"&orderId={ipnRequest.OrderId}" +
                $"&orderInfo={ipnRequest.OrderInfo}" +
                $"&orderType={ipnRequest.OrderType}" +
                $"&transId={ipnRequest.TransId}" +
                $"&message={ipnRequest.Message}" +
                $"&localMessage={ipnRequest.LocalMessage}" +
                $"&responseTime={ipnRequest.ResponseTime}" +
                $"&errorCode={ipnRequest.ErrorCode}" +
                $"&extraData={ipnRequest.ExtraData}";

            var expectedSignature = ComputeHmacSha256(rawHash, _options.SecretKey);

            var isValid = string.Equals(
                expectedSignature,
                ipnRequest.Signature,
                StringComparison.OrdinalIgnoreCase);

            if (!isValid)
            {
                _logger.LogWarning(
                    "Momo IPN signature mismatch. OrderId={OrderId}, Expected={Expected}, Received={Received}",
                    ipnRequest.OrderId, expectedSignature, ipnRequest.Signature);
            }

            return isValid;
        }

        private static string ComputeHmacSha256(string rawData, string secretKey)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <inheritdoc/>
        public async Task<MomoQueryTransactionResponse> QueryTransactionAsync(
            string orderId,
            CancellationToken cancellationToken = default)
        {
            var requestId = Guid.NewGuid().ToString();
            var extraData = string.Empty;

            // V2 query API signature — alphabetical order, no requestType in signature
            // Ref: https://developers.momo.vn/v3/docs/payment/api/transaction-status
            var rawHash =
                $"accessKey={_options.AccessKey}" +
                $"&orderId={orderId}" +
                $"&partnerCode={_options.PartnerCode}" +
                $"&requestId={requestId}";

            var signature = ComputeHmacSha256(rawHash, _options.SecretKey);

            _logger.LogInformation("Momo query raw hash: {RawHash}", rawHash);
            _logger.LogInformation("Momo query signature: {Signature}", signature);

            var requestBody = new
            {
                partnerCode = _options.PartnerCode,
                accessKey = _options.AccessKey,
                requestId,
                orderId,
                requestType = "transactionStatus",
                lang = "vi",
                signature
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("Querying Momo transaction status: OrderId={OrderId}", orderId);

            // Use V2 query endpoint, not the legacy payment creation endpoint
            var response = await _httpClient.PostAsync(_options.QueryEndpoint, jsonContent, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("Momo transaction query response: {Response}", responseContent);

            var result = JsonSerializer.Deserialize<MomoQueryTransactionResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? throw new InvalidOperationException("Failed to deserialize Momo transaction query response.");

            return result;
        }
    }
}
