using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("article_tags")]
    public class ArticleTag
    {
        [Key]
        [Column("id", TypeName = "integer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("news_article_id", TypeName = "integer")]
        public int NewsArticleId { get; set; }

        [Required]
        [Column("ticker", TypeName = "varchar(20)")]
        public string Ticker { get; set; } = string.Empty;

        [Column("relevance_score", TypeName = "numeric(5, 2)")]
        public decimal? RelevanceScore { get; set; }

        [Column("sentiment_score", TypeName = "numeric(5, 2)")]
        public decimal? SentimentScore { get; set; }

        // Navigation Properties
        [ForeignKey("NewsArticleId")]
        public virtual NewsArticle NewsArticle { get; set; } = default!;

        [ForeignKey("Ticker")]
        public virtual Symbol Symbol { get; set; } = default!;
    }
}
