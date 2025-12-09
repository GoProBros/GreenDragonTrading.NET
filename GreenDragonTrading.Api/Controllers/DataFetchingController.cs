using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSectorsFromSsi;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSymbolsFromSsiV1;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSymbolsFromSsiV2;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.MapSymbolSector;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    [Route("api/data-fetching")]
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
        [HttpPost("v1/import-sectors-from-ssi")]
        public async Task<ActionResult<ApiResponse<ImportSectorsFromSsiResult>>> ImportSectorsFromSsi(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ImportSectorsFromSsiCommand(), cancellationToken);
            return Ok(ApiResponse<ImportSectorsFromSsiResult>.SuccessResponse(result, result.Message));
        }

        /// <summary>
        /// Imports symbol data (stocks, ETFs, bonds) from SSI API for all exchanges (HSX, HNX, UPCOM).
        /// Compares with existing records and performs bulk insert/update operations.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>An API response containing the count of imported and updated symbols.</returns>
        [HttpPost("v1/import-symbols-from-ssi")]
        public async Task<ActionResult<ApiResponse<ImportSymbolsFromSsiV1Result>>> ImportSymbolsFromSsi(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ImportSymbolsFromSsiCommandV1(), cancellationToken);
            return Ok(ApiResponse<ImportSymbolsFromSsiV1Result>.SuccessResponse(result, result.Message));
        }

        /// <summary>
        /// Imports symbol data (stocks, ETFs) from SSI API for all exchanges (HSX, HNX, UPCOM).
        /// Compares with existing records and performs bulk insert/update operations.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>An API response containing the count of imported and updated symbols.</returns>
        [HttpPost("v2/import-symbols-from-ssi")]
        public async Task<ActionResult<ApiResponse<ImportSymbolsFromSsiV2Result>>> ImportSymbolsFromSsiV2(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ImportSymbolsFromSsiCommandV2(), cancellationToken);
            return Ok(ApiResponse<ImportSymbolsFromSsiV2Result>.SuccessResponse(result, result.Message));
        }

        /// <summary>
        /// Maps existed symbols to sectors based on SSI data.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>An API response containing the count of mapped symbols.</returns>
        [HttpPost("v1/map-symbols-sector-from-ssi")]
        public async Task<ActionResult<ApiResponse<MapSymbolSectorCommandResult>>> MapSymbolSectorFromSsi(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new MapSymbolSectorCommand(), cancellationToken);
            return Ok(ApiResponse<MapSymbolSectorCommandResult>.SuccessResponse(result, result.Message));
        }
    }
}
