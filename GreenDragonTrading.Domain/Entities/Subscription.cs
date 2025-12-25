using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("Subscriptions")]
    public class Subscription
    {
        [Key]
        [Column("id", TypeName = "integer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } 

        [Required]
        [MaxLength(50)]
        [Column("name", TypeName = "varchar(50)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("level_order", TypeName = "integer")]
        public SubscriptionLevel LevelOrder { get; set; }

        [Required]
        [Column("max_layouts", TypeName = "integer")]
        public int MaxLayouts { get; set; }

        [Required]
        [Column("price", TypeName = "numeric(18, 2)")]
        public decimal Price { get; set; }

        [Required]
        [Column("duration_in_days", TypeName = "integer")]
        public int DurationInDays { get; set; }

        public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}
