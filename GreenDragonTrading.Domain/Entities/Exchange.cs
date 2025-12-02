using GreenDragonTrading.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    /// <summary>
    /// Sàn chứng khoán
    /// </summary>
    [Table("exchanges")]
    public class Exchange
    {
        [Key]
        [Column("code", TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string Code { get; set; } = null!;

        [Column("name", TypeName = "varchar(100)")]
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Biên độ giao động
        /// </summary>
        [Column("normal_fluctuation_limit", TypeName = "decimal(5,4)")]
        [Required]
        public decimal NormalFluctuationLimit { get; set; }

        [Column("first_day_fluctuation_limit", TypeName = "decimal(5,4)")]
        [Required]
        public decimal FirstDayFluctuationLimit { get; set; }

        [Column("no_rights_fluctuation_limit", TypeName = "decimal(5,4)")]
        [Required]
        public decimal NoRightsFluctuationLimit { get; set; }

        [Column("status", TypeName = "smallint")]
        [Required]
        public CommonStatus Status { get; set; } = CommonStatus.Active;

        public virtual ICollection<Symbol> Symbols { get; set; } = [];
    }
}
