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
    }

    public class PaymentInformationResponse
    {
        public long OrderCode { get; set; }
        public int Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public string? CancellationReason { get; set; }
    }
}
