using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace GreenDragonTrading.Domain.Entities
{
    /// <summary>
    /// Nhóm ngành
    /// </summary>
    [Table("sectors")]
    public class Sector
    {
        [Key]
        [Column("id", TypeName = "varchar(10)")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; } = null!;

        [Column("en_name", TypeName = "varchar(100)")]
        [MaxLength(100)]
        public string? EnName { get; set; }

        [Column("vi_name", TypeName = "varchar(100)")]
        [MaxLength(100)]
        public string? ViName { get; set; }

        [Column("parent_id", TypeName = "varchar(10)")]
        public string? ParentId { get; set; }

        [Column("level")]
        public int? Level { get; set; }

        // Navigation Properties
        [ForeignKey("ParentId")]
        public virtual Sector? ParentSector { get; set; }

        public virtual ICollection<Sector> ChildSectors { get; set; } = [];

        public virtual ICollection<Symbol> Symbols { get; set; } = [];
    }
}
