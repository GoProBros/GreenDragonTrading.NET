using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Portfolios.Commands.CreatePortfolio;
using GreenDragonTrading.Application.UseCases.Portfolios.Commands.DeletePortfolio;
using GreenDragonTrading.Application.UseCases.Portfolios.Commands.UpdateMyInvestmentCapital;
using GreenDragonTrading.Application.UseCases.Portfolios.Commands.UpdatePortfolio;
using GreenDragonTrading.Application.UseCases.Portfolios.Queries.GetPortfolioById;
using GreenDragonTrading.Application.UseCases.Portfolios.Queries.GetPortfolios;
using GreenDragonTrading.Application.UseCases.TradingTransactions.Commands.CreateTradingTransaction;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers;

/// <summary>
/// Portfolio management endpoints.
/// </summary>
/// <remarks>
/// Portfolio status values:
/// - 1 = Active
/// - 0 = InActive
/// 
/// Trading transaction side values:
/// - 1 = Buy
/// - 2 = Sell
/// </remarks>
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

    /// <summary>
    /// Gets portfolios with paging, ticker search, and overall profit/loss filter.
    /// Results are always sorted by overall PnL in descending order.
    /// </summary>
    /// <param name="userId">Optional target user id. Only for Admin/Staff. Ignored for User role.</param>
    /// <param name="ticker">Optional ticker keyword for search (case-insensitive).</param>
    /// <param name="overallFilter">Optional overall PnL sign filter: 1 = Profit (greater than 0), 2 = Loss (less than 0).</param>
    /// <param name="status">Optional portfolio status filter: 1 = Active, 0 = InActive. Only applied for Admin/Staff.</param>
    /// <param name="pageIndex">Page number, starting from 1.</param>
    /// <param name="pageSize">Page size between 1 and 100.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated portfolio list.</returns>
    [HttpGet]
    [Authorize(Roles = $"{nameof(UserRole.User)},{nameof(UserRole.Admin)},{nameof(UserRole.Staff)}")]
    public async Task<ActionResult<ApiResponse<PortfolioListResponseDto>>> GetPortfolios(
        [FromQuery] Guid? userId,
        [FromQuery] string? ticker,
        [FromQuery] PortfolioOverallFilter? overallFilter,
        [FromQuery] CommonStatus? status = null,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPortfoliosQuery(userId, ticker, overallFilter, status)
        {
            PageIndex = pageIndex,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
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

    /// <summary>
    /// Creates a new trading transaction for a portfolio.
    /// </summary>
    /// <param name="portfolioId">Portfolio identifier.</param>
    /// <param name="command">Transaction payload. Side values: 1 = Buy, 2 = Sell.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created trading transaction.</returns>
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

    [HttpPatch("investment-capital")]
    [Authorize(Roles = nameof(UserRole.User))]
    public async Task<ActionResult<ApiResponse<UserInvestmentCapitalDto>>> UpdateMyInvestmentCapital(
        [FromBody] UpdateMyInvestmentCapitalCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
