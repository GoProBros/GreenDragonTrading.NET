using System.Text.Json.Serialization;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs
{
    public class PaymentLinkResponse
    {
        public string CheckoutUrl { get; set; } = string.Empty;
        public long OrderCode { get; set; }
    }

    public class WebhookUpdateResult
    {
        public bool IsSuccess { get; set; }
        public long OrderCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class PaymentStatusResponse
    {
        public long OrderCode { get; set; }
        public decimal Amount { get; set; }
        public TransactionStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int SubscriptionId { get; set; }
        public string SubscriptionName { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class CreatePaymentLinkRequest
    {
        public int SubscriptionId { get; set; }

        public PaymentType PaymentProvider { get; set; } = PaymentType.Payos;
    }

    public class PaymentInformationResponse
    {
        public long OrderCode { get; set; }
        public int Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public string? CancellationReason { get; set; }
    }

    public class MomoCreatePaymentResponse
    {
        public string RequestId { get; set; } = string.Empty;
        public int ErrorCode { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string LocalMessage { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string PayUrl { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }

    public class MomoIpnRequest
    {
        [JsonPropertyName("partnerCode")]
        public string PartnerCode { get; set; } = string.Empty;

        [JsonPropertyName("accessKey")]
        public string AccessKey { get; set; } = string.Empty;

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public string Amount { get; set; } = string.Empty;

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("orderInfo")]
        public string OrderInfo { get; set; } = string.Empty;

        [JsonPropertyName("orderType")]
        public string OrderType { get; set; } = string.Empty;

        [JsonPropertyName("transId")]
        public string TransId { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("localMessage")]
        public string LocalMessage { get; set; } = string.Empty;

        [JsonPropertyName("responseTime")]
        public string ResponseTime { get; set; } = string.Empty;

        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; } = string.Empty;

        [JsonPropertyName("extraData")]
        public string ExtraData { get; set; } = string.Empty;

        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;
    }

    public class MomoReturnRequest
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string OrderInfo { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public string TransId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string LocalMessage { get; set; } = string.Empty;
        public string ResponseTime { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string ExtraData { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }

    public class MomoPaymentLinkResponse
    {
        public string PayUrl { get; set; } = string.Empty;

        public string OrderId { get; set; } = string.Empty;
    }

    public class MomoQueryTransactionResponse
    {
        [JsonPropertyName("partnerCode")]
        public string PartnerCode { get; set; } = string.Empty;

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("resultCode")]
        public int ResultCode { get; set; }

        [JsonPropertyName("transId")]
        public long TransId { get; set; }

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("payType")]
        public string PayType { get; set; } = string.Empty;

        [JsonPropertyName("extraData")]
        public string ExtraData { get; set; } = string.Empty;

        [JsonPropertyName("responseTime")]
        public long ResponseTime { get; set; }
    }
}
