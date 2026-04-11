using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.News.Queries.GetNews;

/// <summary>
/// Handler for GetNewsQuery.
/// </summary>
public class GetNewsQueryHandler(
    IUnitOfWork uow,
    ILogger<GetNewsQueryHandler> logger) : IRequestHandler<GetNewsQuery, ApiResponse<PaginatedResponse<NewsArticleDto>>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ILogger<GetNewsQueryHandler> _logger = logger;

    public async Task<ApiResponse<PaginatedResponse<NewsArticleDto>>> Handle(GetNewsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting news list with Search={Search}, Ticker={Ticker}, PageIndex={PageIndex}, PageSize={PageSize}",
            request.Search,
            request.Ticker,
            request.PageIndex,
            request.PageSize);

        var (articles, totalCount) = await _uow.NewsArticles.GetPaginatedAsync(
            request.Search,
            request.Ticker,
            request.PageIndex,
            request.PageSize,
            cancellationToken);

        var items = articles
            .Select(x =>
            {
                var tickerScores = x.ArticleTags
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
                    Id = x.Id,
                    Title = x.Title ?? string.Empty,
                    Summary = x.Summary,
                    Link = x.Link,
                    ThumbnailUrl = x.ThumbnailUrl,
                    PublishedAt = x.PublishedAt,
                    Tickers = tickerScores
                        .Select(t => t.Ticker)
                        .ToList(),
                    TickerScores = tickerScores
                };
            })
            .ToList();

        var paginated = PaginatedResponse<NewsArticleDto>.Create(
            items,
            totalCount,
            request.PageIndex,
            request.PageSize);

        return ApiResponse<PaginatedResponse<NewsArticleDto>>.Success(
            paginated,
            "Lấy danh sách tin tức thành công.");
    }
}
