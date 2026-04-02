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
