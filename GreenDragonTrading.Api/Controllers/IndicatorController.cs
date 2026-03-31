using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Indicators.Queries.GetIndicatorSnapshot;
using GreenDragonTrading.Application.UseCases.Indicators.Queries.GetIndicatorSnapshotZScore;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Endpoints for reading indicator snapshots from Redis.
    /// </summary>
    [Route("api/v1/indicators")]
    [ApiController]
    public class IndicatorController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Returns the latest raw indicator snapshot from Redis.
        /// </summary>
        /// <param name="ticker">Ticker symbol (e.g. FPT, VNM).</param>
        /// <param name="timeframe">Indicator timeframe (default: D1).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpGet("{ticker}")]
        public async Task<ActionResult<ApiResponse<IndicatorSnapshotDto>>> GetRawIndicator(
            string ticker,
            [FromQuery] string timeframe = "D1",
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new GetIndicatorSnapshotQuery(ticker, timeframe),
                cancellationToken);

            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Returns the latest indicator snapshot normalized to z-score values.
        /// </summary>
        /// <param name="ticker">Ticker symbol (e.g. FPT, VNM).</param>
        /// <param name="timeframe">Indicator timeframe (default: D1).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpGet("{ticker}/zscore")]
        public async Task<ActionResult<ApiResponse<IndicatorSnapshotZScoreDto>>> GetZScoreIndicator(
            string ticker,
            [FromQuery] string timeframe = "D1",
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new GetIndicatorSnapshotZScoreQuery(ticker, timeframe),
                cancellationToken);

            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
    }
}
