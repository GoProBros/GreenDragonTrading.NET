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

        [Column("level_order", TypeName = "integer")]
        public SubscriptionLevel? LevelOrder { get; set; }

        [Column("max_workspaces", TypeName = "integer")]
        public int? MaxWorkspaces { get; set; }

        [Column("price", TypeName = "numeric(18, 2)")]
        public decimal? Price { get; set; }

        [Column("duration_in_days", TypeName = "integer")]
        public int? DurationInDays { get; set; }

        [Required]
        [Column("is_active", TypeName = "smallint")]
        public CommonStatus IsActive { get; set; } = CommonStatus.Active;

        [Required]
        [Column("is_free", TypeName = "boolean")]
        public bool IsFree { get; set; } = false;

        [Required]
        [Column("is_admin", TypeName = "boolean")]
        public bool IsAdmin { get; set; } = false;

        [Required]
        [Column("allowed_modules", TypeName = "jsonb")]
        public string AllowedModules { get; set; } = "[]";

        public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}
