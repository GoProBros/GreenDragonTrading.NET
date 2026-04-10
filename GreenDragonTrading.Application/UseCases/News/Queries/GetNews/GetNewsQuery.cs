using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.News.Queries.GetNews;

/// <summary>
/// Query to retrieve paginated news articles.
/// </summary>
/// <param name="Search">Keyword filter for title, summary, or content.</param>
/// <param name="Ticker">Optional ticker filter (e.g. VCB, FPT).</param>
public record GetNewsQuery(
    string? Search = null,
    string? Ticker = null
) : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<NewsArticleDto>>>;
