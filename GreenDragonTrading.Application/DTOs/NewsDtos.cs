namespace GreenDragonTrading.Application.DTOs;

public class RssNewsItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public string? Summary { get; set; }
}

public class NewsArticleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Link { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public List<string> Tickers { get; set; } = [];
}
