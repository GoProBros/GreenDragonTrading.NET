using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportDailyOhlcvFromSsi;
using GreenDragonTrading.Application.UseCases.Ohlcv.Commands.ImportIntradayOhlcvFromSsi;
using GreenDragonTrading.Application.UseCases.Ohlcv.Queries.GetOhlcv;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Controller để quản lý OHLCV (Open, High, Low, Close, Volume) data
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OhlcvController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OhlcvController> _logger;

        public OhlcvController(IMediator mediator, ILogger<OhlcvController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Lấy OHLCV data theo ticker và timeframe
        /// </summary>
        /// <param name="ticker">Mã chứng khoán (VD: VNM, FPT)</param>
        /// <param name="timeframe">Khung thời gian: M1, M5, M15, M30, H1, H4, D1, W1, MN1</param>
        /// <param name="from">Từ ngày (ISO format)</param>
        /// <param name="to">Đến ngày (ISO format)</param>
        /// <param name="limit">Số lượng nến tối đa (alternative to from/to)</param>
        /// <param name="useCache">Có sử dụng cache không (default: true)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>OHLCV data</returns>
        [HttpGet("{ticker}")]
        public async Task<IActionResult> GetOhlcv(
            string ticker,
            [FromQuery] string timeframe = "D1",
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int? limit = null,
            [FromQuery] bool useCache = true,
            CancellationToken cancellationToken = default)
        {
            var query = new GetOhlcvQuery
            {
                Ticker = ticker.ToUpper(),
                Timeframe = timeframe.ToUpper(),
                FromTime = from ?? DateTime.UtcNow.AddMonths(-1),
                ToTime = to ?? DateTime.UtcNow,
                Limit = limit,
                UseCache = useCache
            };

            var result = await _mediator.Send(query, cancellationToken);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }



        /// <summary>
        /// Import intraday OHLCV (M1) data từ SSI
        /// </summary>
        /// <param name="request">Import request parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Import result</returns>
        [HttpPost("import/intraday")]
        public async Task<IActionResult> ImportIntradayOhlcv(
            [FromBody] ImportOhlcvDto request,
            CancellationToken cancellationToken = default)
        {
            var command = new ImportIntradayOhlcvFromSsiCommand
            {
                Ticker = request.Ticker,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            };

            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Import daily OHLCV (D1) data từ SSI
        /// </summary>
        /// <param name="request">Import request parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Import result</returns>
        [HttpPost("import/daily")]
        public async Task<IActionResult> ImportDailyOhlcv(
            [FromBody] ImportOhlcvDto request,
            CancellationToken cancellationToken = default)
        {
            var command = new ImportDailyOhlcvFromSsiCommand
            {
                Ticker = request.Ticker,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            };

            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Import daily OHLCV cho nhiều tickers (batch)
        /// </summary>
        /// <param name="tickers">Danh sách mã chứng khoán</param>
        /// <param name="fromDate">Từ ngày</param>
        /// <param name="toDate">Đến ngày</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Batch import result</returns>
        [HttpPost("import/daily/batch")]
        public async Task<IActionResult> ImportDailyOhlcvBatch(
            [FromBody] string[] tickers,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            CancellationToken cancellationToken = default)
        {
            var results = new List<ImportOhlcvResultDto>();

            foreach (var ticker in tickers)
            {
                var command = new ImportDailyOhlcvFromSsiCommand
                {
                    Ticker = ticker,
                    FromDate = fromDate,
                    ToDate = toDate
                };

                var result = await _mediator.Send(command, cancellationToken);

                if (result.Data != null)
                {
                    results.Add(result.Data);
                }
            }

            return Ok(new
            {
                TotalTickers = tickers.Length,
                Results = results,
                TotalImported = results.Sum(r => r.RecordsImported),
                TotalSkipped = results.Sum(r => r.RecordsSkipped)
            });
        }
    }
}