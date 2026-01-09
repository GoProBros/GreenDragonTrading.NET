using GreenDragonTrading.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("workspaces")] 
    public class Workspace
    {
        [Key]
        [Column("id", TypeName = "integer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("user_id", TypeName = "uuid")]
        public Guid? UserId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("workspace_name", TypeName = "varchar(100)")]
        public string WorkspaceName { get; set; } = string.Empty;

        [Required]
        [Column("layout_json", TypeName = "jsonb")]
        public string LayoutJson { get; set; } = "{}";

        [Required]
        [Column("is_default", TypeName = "boolean")]
        public bool IsDefault { get; set; } = false;

        [MaxLength(8)]
        [Column("share_code", TypeName = "varchar(8)")]
        public string? ShareCode { get; set; }

        [Required]
        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Column("updated_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;



        // Navigation Property
        [ForeignKey("UserId")]
        public User? User { get; set; } = default!;
    }
}
