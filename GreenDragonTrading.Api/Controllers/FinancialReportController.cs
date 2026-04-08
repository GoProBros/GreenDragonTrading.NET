using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.FinancialReports.Commands.CreateFinancialReport;
using GreenDragonTrading.Application.UseCases.FinancialReports.Commands.DeleteFinancialReport;
using GreenDragonTrading.Application.UseCases.FinancialReports.Commands.UpdateFinancialReport;
using GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReportById;
using GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReportIndicatorList;
using GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetRecentQuarterIndicators;
using GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReports;
using GreenDragonTrading.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Controller for managing financial report operations.
    /// </summary>
    [Route("api/v1/financial-reports")]
    [ApiController]
    public class FinancialReportController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Retrieves a paginated list of financial reports with optional filters.
        /// </summary>
        /// <param name="query">Query parameters for filtering and paginating financial reports.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>A paginated list of financial reports.</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<FinancialReportDto>>>> GetFinancialReports(
            [FromQuery] GetFinancialReportsQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of financial report indicators with filters,
        /// aligned with the main financial reports list endpoint.
        /// </summary>
        /// <param name="query">Filter and pagination query.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>Paginated indicator list.</returns>
        [HttpGet("indicators")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<FinancialReportIndicatorListItemDto>>>> GetFinancialReportIndicatorsWithFilters(
            [FromQuery] GetFinancialReportIndicatorListQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific financial report by ID.
        /// </summary>
        /// <param name="id">The ID of the financial report to retrieve.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>The financial report details.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<FinancialReportDto>>> GetFinancialReportById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetFinancialReportByIdQuery(id), cancellationToken);
            
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Retrieves recent quarterly indicator snapshots for a ticker.
        /// </summary>
        /// <param name="ticker">Ticker symbol.</param>
        /// <param name="count">Number of recent quarters to return. Default is 5.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>Indicator snapshots of recent quarters.</returns>
        [HttpGet("{ticker}/indicators/recent-quarters")]
        public async Task<ActionResult<ApiResponse<IReadOnlyCollection<QuarterIndicatorItemDto>>>> GetRecentQuarterIndicators(
            [FromRoute] string ticker,
            [FromQuery] int count = 5,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new GetRecentQuarterIndicatorsQuery(ticker, count),
                cancellationToken);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Creates a new financial report with optional file upload.
        /// </summary>
        /// <param name="request">The financial report creation request.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>The created financial report.</returns>
        [HttpPost]
        [Consumes("application/json")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<FinancialReportDto>>> CreateFinancialReport(
            [FromBody] CreateFinancialReportRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = new CreateFinancialReportCommand(
                request.Ticker,
                request.Year,
                request.Period,
                request.ReportData
            );

            var result = await _mediator.Send(command, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            
            return CreatedAtAction(
                nameof(GetFinancialReportById),
                new { id = result.Data!.Id },
                result);
        }

        /// <summary>
        /// Updates an existing financial report.
        /// </summary>
        /// <param name="id">The ID of the financial report to update.</param>
        /// <param name="request">The financial report update request.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>The updated financial report.</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<FinancialReportDto>>> UpdateFinancialReport(
            [FromRoute] Guid id,
            [FromBody] UpdateFinancialReportRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = new UpdateFinancialReportCommand(
                id,
                request.ReportData,
                request.Status
            );

            var result = await _mediator.Send(command, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Deletes a financial report.
        /// </summary>
        /// <param name="id">The ID of the financial report to delete.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>Success or failure response.</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> DeleteFinancialReport(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new DeleteFinancialReportCommand(id), cancellationToken);
            
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }
            
            return Ok(result);
        }
    }
}
