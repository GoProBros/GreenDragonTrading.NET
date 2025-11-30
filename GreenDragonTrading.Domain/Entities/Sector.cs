using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    /// <summary>
    /// Nhóm ngành
    /// </summary>
    [Table("sectors")]
    public class Sector
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Column("en_name", TypeName = "varchar(100)")]
        [MaxLength(100)]
        public string? EnName { get; set; }

        [Column("vi_name", TypeName = "varchar(100)")]
        [MaxLength(100)]
        public string? ViName { get; set; }

        [Column("parent_id")]
        public int? ParentId { get; set; }

        [Column("level")]
        public int? Level { get; set; }

        // Navigation Properties
        [ForeignKey("ParentId")]
        public virtual Sector? ParentSector { get; set; }

        public virtual ICollection<Sector> ChildSectors { get; set; } = [];

        public virtual ICollection<Symbol> Symbols { get; set; } = [];
    }
}
