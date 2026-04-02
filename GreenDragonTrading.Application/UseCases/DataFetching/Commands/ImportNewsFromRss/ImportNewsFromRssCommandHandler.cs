using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportNewsFromRss;

public class ImportNewsFromRssCommandHandler(
    INewsRssService newsRssService,
    IAiChatService aiChatService,
    IUnitOfWork uow,
    ILogger<ImportNewsFromRssCommandHandler> logger)
    : IRequestHandler<ImportNewsFromRssCommand, ImportNewsFromRssResult>
{
    private readonly INewsRssService _newsRssService = newsRssService;
    private readonly IAiChatService _aiChatService = aiChatService;
    private readonly IUnitOfWork _uow = uow;
    private readonly ILogger<ImportNewsFromRssCommandHandler> _logger = logger;

    public async Task<ImportNewsFromRssResult> Handle(ImportNewsFromRssCommand request, CancellationToken cancellationToken)
    {
        var rssItems = await _newsRssService.FetchCafeFStockNewsAsync(cancellationToken);
        if (rssItems.Count == 0)
        {
            return new ImportNewsFromRssResult(0, 0, 0, "Không có tin tức mới từ RSS.");
        }

        var links = rssItems
            .Select(x => x.Link)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingLinks = await _uow.NewsArticles.GetExistingLinksAsync(links, cancellationToken);
        var existingLinkSet = new HashSet<string>(existingLinks, StringComparer.OrdinalIgnoreCase);

        var newItems = rssItems
            .Where(x => !existingLinkSet.Contains(x.Link))
            .ToList();

        var validTickers = (await _uow.Symbols.GetAllTickersAsync(cancellationToken))
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var articlesToInsert = new List<NewsArticle>(newItems.Count);

        foreach (var item in newItems)
        {
            string? articleSummary = item.Summary;
            var articleTags = new List<ArticleTag>();

            if (!string.IsNullOrWhiteSpace(item.Content))
            {
                var summaryResponse = await _aiChatService.SummarizeNewsAsync(item.Title, item.Content, cancellationToken);
                articleSummary = summaryResponse?.Summary;

                if (summaryResponse?.Entities != null)
                {
                    var deduplicatedEntities = summaryResponse.Entities
                        .Where(e => !string.IsNullOrWhiteSpace(e.Ticker))
                        .GroupBy(e => e.Ticker!.Trim(), StringComparer.OrdinalIgnoreCase)
                        .Select(g => g.First());

                    foreach (var entity in deduplicatedEntities)
                    {
                        var ticker = entity.Ticker!.Trim().ToUpperInvariant();
                        if (!validTickers.Contains(ticker))
                        {
                            continue;
                        }

                        articleTags.Add(new ArticleTag
                        {
                            Ticker = ticker,
                            RelevanceScore = entity.RelevanceScore,
                            SentimentScore = entity.SentimentScore
                        });
                    }
                }
            }

            articlesToInsert.Add(new NewsArticle
            {
                Title = item.Title,
                Content = item.Content,
                Summary = articleSummary,
                Link = item.Link,
                ThumbnailUrl = item.ThumbnailUrl,
                PublishedAt = item.PublishedAt.ToUniversalTime(),
                ArticleTags = articleTags
            });
        }

        if (articlesToInsert.Count > 0)
        {
            await _uow.NewsArticles.AddRangeAsync(articlesToInsert, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
        }

        var duplicatedCount = rssItems.Count - articlesToInsert.Count;

        _logger.LogInformation(
            "Imported RSS news completed. Fetched={FetchedCount}, Inserted={InsertedCount}, Duplicated={DuplicatedCount}",
            rssItems.Count,
            articlesToInsert.Count,
            duplicatedCount);

        return new ImportNewsFromRssResult(
            rssItems.Count,
            articlesToInsert.Count,
            duplicatedCount,
            $"Import thành công: {articlesToInsert.Count} bài mới, {duplicatedCount} bài trùng.");
    }
}
