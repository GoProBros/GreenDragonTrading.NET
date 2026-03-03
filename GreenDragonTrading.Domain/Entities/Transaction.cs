using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("transactions")]
    public class Transaction
    {
        [Key]
        [Column("id", TypeName = "uuid")]
        public Guid Id { get; set; }

        [Required]
        [Column("order_code", TypeName = "bigint")]
        public long OrderCode { get; set; } 

        [Column("user_id", TypeName = "uuid")]
        public Guid UserId { get; set; }

        [Column("subscription_id", TypeName = "integer")]
        public int SubscriptionId { get; set; }

        [Column("amount", TypeName = "numeric(18, 2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("status", TypeName = "smallint")]
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        [Required]
        [Column("transaction_type", TypeName = "smallint")]
        public TransactionType Type { get; set; } = TransactionType.Purchase;

        [Required]
        [Column("payment_provider", TypeName = "smallint")]
        public PaymentType PaymentProvider { get; set; }

        [Column("provider_transaction_id", TypeName = "varchar(255)")]
        public string? ProviderTransactionId { get; set; } 

        [Column("checkout_url", TypeName = "varchar(255)")]
        public string? CheckoutUrl { get; set; }

        [MaxLength(255)]
        [Column("description", TypeName = "varchar(255)")]
        public string? Description { get; set; }

        [Required]
        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [ForeignKey("UserId")]
        public User User { get; set; } = default!;

        [ForeignKey("SubscriptionId")]
        public Subscription Subscription { get; set; } = default!;
    }
}