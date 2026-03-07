using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportFromDnse;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportIndexConstituentsFromSsi;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSectorsFromSsi;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSpecificPeriodFromDnse;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSymbolsFromSsiV1;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSymbolsFromSsiV2;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.MapSymbolSector;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Fetches data from external APIs and imports into the system.
    /// </summary>
    /// <param name="mediator">Mediator service</param>
    [Route("api/v1/data-fetching")]
    [ApiController]
    public class DataFetchingController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Imports sector/industry data from SSI API into the database.
        /// Fetches all industry levels and performs bulk insert/update operations.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>An API response containing the count of imported and updated sectors.</returns>
        [HttpPost("import-sectors-from-ssi")]
        public async Task<ActionResult<ApiResponse<ImportSectorsFromSsiResult>>> ImportSectorsFromSsi(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ImportSectorsFromSsiCommand(), cancellationToken);
            return Ok(ApiResponse<ImportSectorsFromSsiResult>.Success(result, result.Message));
        }

        /// <summary>
        /// Imports symbol data (stocks, ETFs, bonds) from SSI API for all exchanges (HSX, HNX, UPCOM).
        /// Compares with existing records and performs bulk insert/update operations.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>An API response containing the count of imported and updated symbols.</returns>
        [HttpPost("import-symbols-from-ssi")]
        public async Task<ActionResult<ApiResponse<ImportSymbolsFromSsiV1Result>>> ImportSymbolsFromSsi(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ImportSymbolsFromSsiCommandV1(), cancellationToken);
            return Ok(ApiResponse<ImportSymbolsFromSsiV1Result>.Success(result, result.Message));
        }

        /// <summary>
        /// Imports symbol data (stocks, ETFs) from SSI API for all exchanges (HSX, HNX, UPCOM).
        /// Compares with existing records and performs bulk insert/update operations.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>An API response containing the count of imported and updated symbols.</returns>
        [HttpPost("~/api/v2/data-fetching/import-symbols-from-ssi")]
        public async Task<ActionResult<ApiResponse<ImportSymbolsFromSsiV2Result>>> ImportSymbolsFromSsiV2(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ImportSymbolsFromSsiCommandV2(), cancellationToken);
            return Ok(ApiResponse<ImportSymbolsFromSsiV2Result>.Success(result, result.Message));
        }

        /// <summary>
        /// Maps existed symbols to sectors based on SSI data.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>An API response containing the count of mapped symbols.</returns>
        [HttpPost("map-symbols-sector-from-ssi")]
        public async Task<ActionResult<ApiResponse<MapSymbolSectorCommandResult>>> MapSymbolSectorFromSsi(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new MapSymbolSectorCommand(), cancellationToken);
            return Ok(ApiResponse<MapSymbolSectorCommandResult>.Success(result, result.Message));
        }

        /// <summary>
        /// Import bulk financial reports from DNSE API.
        /// Imports data for multiple tickers and multiple periods (5 or 10 cycles).
        /// </summary>
        /// <param name="request">Import request containing tickers, cycle type, and cycle number.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>Import result with success/failure statistics.</returns>
        [HttpPost("financial-reports/bulk")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<DnseImportResult>>> ImportBulkFromDnse(
            [FromBody] ImportBulkFromDnseCommand request,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(request, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get financial report for a specific period from DNSE API but not import to database.
        /// </summary>
        /// <param name="request">Import request containing ticker, year, and optional quarter.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>The imported financial report.</returns>
        [HttpPost("financial-reports/specific")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<FinancialReportDto>>> ImportSpecificPeriodFromDnse(
            [FromBody] ImportSpecificPeriodFromDnseCommand request,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(request, cancellationToken);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        /// <summary>
        /// Fetches constituent symbols for all active market indices from SSI API
        /// and syncs them into the <c>market_index_symbols</c> table.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>An API response with the number of indices processed and records upserted.</returns>
        [HttpPost("import-index-constituents-from-ssi")]
        public async Task<ActionResult<ApiResponse<ImportIndexConstituentsFromSsiResult>>> ImportIndexConstituentsFromSsi(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ImportIndexConstituentsFromSsiCommand(), cancellationToken);
            return Ok(ApiResponse<ImportIndexConstituentsFromSsiResult>.Success(result, result.Message));
        }
    }
}
