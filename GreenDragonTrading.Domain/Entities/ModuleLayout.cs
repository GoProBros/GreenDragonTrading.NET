using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table ("module_layouts")]
    public class ModuleLayout
    {
        [Key]
        [Column("id", TypeName = "bigint")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("user_id", TypeName = "uuid")]
        public Guid? UserId { get; set; } 

        [Required]
        [MaxLength(50)]
        [Column("module_type", TypeName = "smallint")]
        public ModuleType ModuleType { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("layout_name", TypeName = "varchar(100)")]
        public string LayoutName { get; set; } = string.Empty; 

        [Required]
        [Column("config_json", TypeName = "jsonb")]
        public string ConfigJson { get; set; } = "{}"; 

        [Required]
        [Column("is_system_default", TypeName = "boolean")]
        public bool IsSystemDefault { get; set; } = false;

        [Required]
        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Column("updated_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;


        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
