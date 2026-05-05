using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetIndexConstituents;
using GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetIntradayIndex;
using GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetLiveIndices;
using GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetMarketIndexById;
using GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetMarketIndices;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Endpoints for querying market indices (VN30, VNINDEX, HNX30, …) and their constituent symbols.
    /// </summary>
    [Route("api/v1/market-indices")]
    [ApiController]
    public class MarketIndexController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Retrieves a paginated, filtered list of market indices.
        /// </summary>
        /// <param name="exchangeCode">Filter by exchange code (e.g. HSX, HNX). Null = all.</param>
        /// <param name="isBenchmark">Filter to benchmark indices only. Null = all.</param>
        /// <param name="status">Status filter. 0 = Inactive, 1 = Active. Default = Active.</param>
        /// <param name="search">Full-text search on Code and Name fields.</param>
        /// <param name="pageIndex">Page number (default: 1).</param>
        /// <param name="pageSize">Items per page (default: 10, max: 100).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated list of market indices.</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<MarketIndexDto>>>> GetMarketIndices(
            [FromQuery] string? exchangeCode = null,
            [FromQuery] bool? isBenchmark = null,
            [FromQuery] CommonStatus? status = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetMarketIndicesQuery(exchangeCode, isBenchmark, status, search)
            {
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a single market index by its code.
        /// </summary>
        /// <param name="code">Index code (e.g. "VN30", "HNX30").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Market index details.</returns>
        [HttpGet("{code}")]
        public async Task<ActionResult<ApiResponse<MarketIndexDto>>> GetMarketIndexByCode(
            string code,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetMarketIndexByIdQuery(code), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the paginated list of constituent symbols for a given market index.
        /// </summary>
        /// <param name="code">Index code (e.g. "VN30").</param>
        /// <param name="isActive">Filter active/inactive constituents. Null = active only.</param>
        /// <param name="search">Search by ticker or company name.</param>
        /// <param name="pageIndex">Page number (default: 1).</param>
        /// <param name="pageSize">Items per page (default: 50, max: 100).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated list of constituent symbols.</returns>
        [HttpGet("{code}/constituents")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<MarketIndexSymbolDto>>>> GetIndexConstituents(
            string code,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var query = new GetIndexConstituentsQuery(code, isActive, search)
            {
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns live index snapshots from Redis for the requested index codes.
        /// Data is updated in real-time by the SSI MI streaming channel.
        /// </summary>
        /// <param name="codes">One or more index codes (e.g. VNINDEX, VN30). Repeatable query param.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of live index snapshots; codes with no cached data are omitted.</returns>
        [HttpGet("live")]
        public async Task<ActionResult<ApiResponse<List<LiveIndexDataDto>>>> GetLiveIndices(
            [FromQuery] string[] codes,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetLiveIndicesQuery(codes), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Returns today's intraday price history for a single market index, ordered chronologically.
        /// Suitable for rendering sparkline charts in the Index Module.
        /// </summary>
        /// <param name="code">Index code (e.g. "VNINDEX").</param>
        /// <param name="maxPoints">Maximum number of data points to return. Omit or pass -1 to return all points for the day.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of time/value pairs in ascending time order.</returns>
        [HttpGet("{code}/intraday")]
        public async Task<ActionResult<ApiResponse<List<IndexHistoryPointDto>>>> GetIndexIntraday(
            string code,
            [FromQuery] int maxPoints = -1,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetIntradayIndexQuery(code, maxPoints), cancellationToken);
            return Ok(result);
        }
    }
}
