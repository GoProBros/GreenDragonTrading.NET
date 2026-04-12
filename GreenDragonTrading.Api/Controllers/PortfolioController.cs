using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Portfolios.Commands.CreatePortfolio;
using GreenDragonTrading.Application.UseCases.Portfolios.Commands.DeletePortfolio;
using GreenDragonTrading.Application.UseCases.Portfolios.Commands.UpdatePortfolio;
using GreenDragonTrading.Application.UseCases.Portfolios.Queries.GetPortfolioById;
using GreenDragonTrading.Application.UseCases.Portfolios.Queries.GetPortfolios;
using GreenDragonTrading.Application.UseCases.TradingTransactions.Commands.CreateTradingTransaction;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers;

[ApiController]
[Route("api/v1/portfolios")]
[Authorize]
public class PortfolioController : ControllerBase
{
    private readonly IMediator _mediator;

    public PortfolioController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = $"{nameof(UserRole.User)},{nameof(UserRole.Admin)},{nameof(UserRole.Staff)}")]
    public async Task<ActionResult<ApiResponse<List<PortfolioDto>>>> GetPortfolios(
        [FromQuery] Guid? userId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPortfoliosQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{nameof(UserRole.User)},{nameof(UserRole.Admin)},{nameof(UserRole.Staff)}")]
    public async Task<ActionResult<ApiResponse<PortfolioDto>>> GetPortfolioById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPortfolioByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.User))]
    public async Task<ActionResult<ApiResponse<PortfolioDto>>> CreatePortfolio(
        [FromBody] CreatePortfolioCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(UserRole.User))]
    public async Task<ActionResult<ApiResponse<PortfolioDto>>> UpdatePortfolio(
        [FromRoute] int id,
        [FromBody] UpdatePortfolioCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Id != id)
        {
            return BadRequest(ApiResponse<PortfolioDto>.Failure("ID không khớp."));
        }

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(UserRole.User))]
    public async Task<ActionResult<ApiResponse>> DeletePortfolio(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeletePortfolioCommand(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{portfolioId:int}/transactions")]
    [Authorize(Roles = nameof(UserRole.User))]
    public async Task<ActionResult<ApiResponse<TradingTransactionDto>>> CreateTradingTransaction(
        [FromRoute] int portfolioId,
        [FromBody] CreateTradingTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var request = command with { PortfolioId = portfolioId };
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}
