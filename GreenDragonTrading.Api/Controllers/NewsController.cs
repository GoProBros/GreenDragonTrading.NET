using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.News.Queries.GetNews;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers;

/// <summary>
/// Endpoints for retrieving stock market news.
/// </summary>
[Route("api/v1/news")]
[ApiController]
public class NewsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves paginated news articles with optional keyword and ticker filters.
    /// </summary>
    /// <param name="query">Pagination and filter query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of news articles.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<NewsArticleDto>>>> GetNews(
        [FromQuery] GetNewsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
