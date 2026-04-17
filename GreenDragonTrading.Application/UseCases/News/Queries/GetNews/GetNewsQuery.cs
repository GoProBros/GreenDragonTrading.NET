using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.News.Queries.GetNews;

/// <summary>
/// Query to retrieve paginated news articles.
/// </summary>
/// <param name="Search">Keyword filter for title, summary, or content.</param>
/// <param name="Ticker">Optional ticker filter (e.g. VCB, FPT).</param>
/// <param name="PublishedToday">If true, returns only articles published today (UTC date).</param>
public record GetNewsQuery(
    string? Search = null,
    string? Ticker = null,
    bool PublishedToday = false
) : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<NewsArticleDto>>>;
