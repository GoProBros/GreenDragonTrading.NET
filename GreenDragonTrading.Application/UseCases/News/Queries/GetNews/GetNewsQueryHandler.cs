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
            "Getting news list with Search={Search}, Ticker={Ticker}, PublishedToday={PublishedToday}, PageIndex={PageIndex}, PageSize={PageSize}",
            request.Search,
            request.Ticker,
            request.PublishedToday,
            request.PageIndex,
            request.PageSize);

        var (articles, totalCount) = await _uow.NewsArticles.GetPaginatedAsync(
            request.Search,
            request.Ticker,
            request.PublishedToday,
            request.PageIndex,
            request.PageSize,
            cancellationToken);

        var items = articles
            .Select(NewsArticleMapper.ToDto)
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
