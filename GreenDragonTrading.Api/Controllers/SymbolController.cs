using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbol;
using GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbols;
using GreenDragonTrading.Application.UseCases.Symbols.Queries.SearchSymbols;
using GreenDragonTrading.Application.UseCases.Symbols.Queries.GetOhlcv;
using GreenDragonTrading.Application.UseCases.Symbols.Queries.GetIntradayOhlc;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Controller for managing symbol-related operations.
    /// </summary>
    [Route("api/v1/symbol")]
    [ApiController]
    public class SymbolController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Retrieves a list of symbols with optional filtering, sorting, and pagination.
        /// </summary>
        /// <param name="query">Query parameters for filtering, searching, and paginating symbol data.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="ApiResponse{T}"/> containing a paginated list of symbols and metadata.</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<SymbolDto>>> GetSymbols([FromQuery] GetSymbolsQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);
            if (!result.IsSuccess) 
            { 
                return NotFound(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Retrieves details information of symbol.
        /// </summary>
        /// <param name="ticker">The ticker symbol to retrieve information for.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="ApiResponse{T}"/> containing the symbol details.</returns>
        [HttpGet("{ticker}")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<SymbolDto>>>> GetSymbol([FromRoute] string ticker, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetSymbolQuery(ticker), cancellationToken);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Search for symbols based on a query string make isTickerOnly flase if you want to search by ticker or companyname.
        /// </summary>
        /// <param name="request">Request model contains search parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<SimpleSymbolDto>>>> SearchSymbols([FromQuery] SearchSymbolsQuery request, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(request, cancellationToken);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Get intraday OHLC data for a specific symbol within a date range.
        /// </summary>
        /// <param name="request">Request model containing symbol and date range information.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [HttpGet("intraday-ohlc")]
        public async Task<ActionResult<PaginatedResponse<IntradayOhlc>>> IntradayOhlc([FromQuery] GetIntradayOhlcQuery request, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(request, cancellationToken);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Get stock chart data (OHLCV) for candlestick visualization.
        /// Fetches OHLC data from Finsc API optimized for charting libraries (TradingView, Highcharts, etc.)
        /// </summary>
        /// <param name="symbol">Stock ticker (e.g., FPT, VNM, SSI)</param>
        /// <param name="resolution">Chart timeframe: 1D (daily), 1H (hourly), 15, 5, 1 (minutes). Default: 1D</param>
        /// <param name="fromDate">Start date in yyyy-MM-dd format (e.g., 2024-01-01). Default: 3 months ago</param>
        /// <param name="toDate">End date in yyyy-MM-dd format (e.g., 2024-12-31). Default: today</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>OHLC data in parallel arrays format (t, o, h, l, c, v, symbol, s)</returns>
        /// <response code="200">Returns chart data successfully</response>
        /// <response code="400">Invalid parameters (e.g., wrong date format)</response>
        [HttpGet("{symbol}/ohlcv")]
        [ProducesResponseType(typeof(ApiResponse<FinscStockResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<FinscStockResponse>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<FinscStockResponse>>> GetOhlcv(
            [FromRoute] string symbol,
            [FromQuery] string? resolution,
            [FromQuery] string? fromDate,
            [FromQuery] string? toDate,
            CancellationToken cancellationToken = default)
        {
            var query = new GetOhlcvQuery(symbol, resolution, fromDate, toDate);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
