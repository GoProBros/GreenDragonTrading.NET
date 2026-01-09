using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.WatchLists.Commands.CreateWatchList;
using GreenDragonTrading.Application.UseCases.WatchLists.Commands.DeleteWatchList;
using GreenDragonTrading.Application.UseCases.WatchLists.Commands.UpdateWatchList;
using GreenDragonTrading.Application.UseCases.WatchLists.Queries.GetMyWatchLists;
using GreenDragonTrading.Application.UseCases.WatchLists.Queries.GetWatchListById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers;

[ApiController]
[Route("api/v1/watch-lists")]
[Authorize]
public class WatchListController : ControllerBase
{
    private readonly IMediator _mediator;

    public WatchListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WatchListListItemDto>>>> GetMyWatchLists(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyWatchListsQuery(), cancellationToken);
        return result;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<WatchListDto>>> GetWatchListById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWatchListByIdQuery(id), cancellationToken);
        return result;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WatchListDto>>> CreateWatchList(
        [FromBody] CreateWatchListCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result;
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<WatchListDto>>> UpdateWatchList(
        [FromRoute] int id,
        [FromBody] UpdateWatchListCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Id != id)
        {
            return BadRequest(ApiResponse<WatchListDto>.Failure("ID không khớp."));
        }

        var result = await _mediator.Send(command, cancellationToken);
        return result;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteWatchList(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteWatchListCommand(id), cancellationToken);
        return result;
    }
}
