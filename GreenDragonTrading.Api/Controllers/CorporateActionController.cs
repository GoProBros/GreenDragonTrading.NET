using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.CorporateActions.Queries.GetCorporateActionById;
using GreenDragonTrading.Application.UseCases.CorporateActions.Queries.GetCorporateActions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers;

/// <summary>
/// Endpoints for retrieving corporate actions.
/// </summary>
[Route("api/v1/corporate-actions")]
[ApiController]
public class CorporateActionController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves paginated corporate actions with optional filters.
    /// </summary>
    /// <param name="query">Pagination and filter query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of corporate actions.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<CorporateActionDto>>>> GetCorporateActions(
        [FromQuery] GetCorporateActionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a corporate action by event id.
    /// </summary>
    /// <param name="eventId">Corporate action event id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Corporate action details.</returns>
    [HttpGet("{eventId:int}")]
    public async Task<ActionResult<ApiResponse<CorporateActionDto>>> GetCorporateActionById(
        [FromRoute] int eventId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCorporateActionByIdQuery(eventId), cancellationToken);
        return Ok(result);
    }
}
