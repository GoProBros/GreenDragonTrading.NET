using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenDragonTrading.Domain.Entities
{
    [Table("news_articles")]
    public class NewsArticle
    {
        [Key]
        [Column("id", TypeName = "integer")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("title", TypeName = "varchar(500)")]
        public string? Title { get; set; } = string.Empty;

        [Column("content", TypeName = "text")]
        public string? Content { get; set; } 

        [Column("summary", TypeName = "text")]
        public string? Summary { get; set; } 

        [Column("link", TypeName = "varchar(1000)")]
        public string? Link { get; set; } = string.Empty;

        [Column("thumbnail_url", TypeName = "varchar(1000)")]
        public string? ThumbnailUrl { get; set; }

        [Column("published_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset PublishedAt { get; set; }

        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public virtual ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
    }
}
