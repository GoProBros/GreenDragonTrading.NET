using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSectorsFromSsi;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSymbolsFromSsi;
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
        public async Task<ActionResult<ApiResponse<ImportSymbolsFromSsiResult>>> ImportSymbolsFromSsi(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new ImportSymbolsFromSsiCommand(), cancellationToken);
            return Ok(ApiResponse<ImportSymbolsFromSsiResult>.SuccessResponse(result, result.Message));
        }
    }
}
