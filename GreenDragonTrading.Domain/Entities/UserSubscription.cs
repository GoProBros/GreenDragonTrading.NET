using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    public class UserSubscription
    {
        [Key]
        [Column("id", TypeName = "integer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("user_id", TypeName = "uuid")]
        public Guid UserId { get; set; }

        [Column("subscription_id", TypeName = "integer")]
        public int SubscriptionId { get; set; }

        [Required]
        [Column("start_date", TypeName = "timestamp with time zone")]
        public DateTimeOffset StartDate { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Column("end_date", TypeName = "timestamp with time zone")]
        public DateTimeOffset EndDate { get; set; }

        [Required]
        [Column("status", TypeName = "smallint")]
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

        // Navigation Properties
        [ForeignKey("UserId")]
        public User User { get; set; } = default!;

        [ForeignKey("SubscriptionId")]
        public Subscription Subscription { get; set; } = default!;
    }
}
