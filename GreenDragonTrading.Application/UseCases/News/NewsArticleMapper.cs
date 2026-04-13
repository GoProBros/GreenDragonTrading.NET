using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Application.UseCases.News;

internal static class NewsArticleMapper
{
    public static NewsArticleDto ToDto(NewsArticle article)
    {
        var tickerScores = article.ArticleTags
            .Where(t => !string.IsNullOrWhiteSpace(t.Ticker))
            .GroupBy(t => t.Ticker.Trim().ToUpperInvariant(), StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var bestTag = g
                    .OrderByDescending(t => t.RelevanceScore.HasValue)
                    .ThenByDescending(t => t.RelevanceScore)
                    .ThenByDescending(t => t.SentimentScore.HasValue)
                    .ThenByDescending(t => t.SentimentScore)
                    .First();

                return new NewsArticleTickerScoreDto
                {
                    Ticker = g.Key,
                    RelevanceScore = bestTag.RelevanceScore,
                    SentimentScore = bestTag.SentimentScore
                };
            })
            .OrderBy(t => t.Ticker)
            .ToList();

        return new NewsArticleDto
        {
            Id = article.Id,
            Title = article.Title ?? string.Empty,
            Summary = article.Summary,
            Link = article.Link,
            Source = ExtractSource(article.Link),
            ThumbnailUrl = article.ThumbnailUrl,
            PublishedAt = article.PublishedAt,
            Tickers = tickerScores.Select(t => t.Ticker).ToList(),
            TickerScores = tickerScores
        };
    }

    private static string? ExtractSource(string? link)
    {
        if (string.IsNullOrWhiteSpace(link))
        {
            return null;
        }

        if (Uri.TryCreate(link, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host))
        {
            return uri.Host;
        }

        var value = link.Trim();
        var schemeSeparatorIndex = value.IndexOf("//", StringComparison.Ordinal);
        if (schemeSeparatorIndex >= 0)
        {
            value = value[(schemeSeparatorIndex + 2)..];
        }

        var slashIndex = value.IndexOf('/');
        return slashIndex >= 0 ? value[..slashIndex] : value;
    }
}
