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

        [HttpPost("v1/import-sectors-from-ssi")]
        public async Task<IActionResult> ImportSectorsFromSsi(CancellationToken cancellationToken = default)
        {
            var command = new ImportSectorsFromSsiCommand();
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(ApiResponse<int>.SuccessResponse(result.ImportedCount, result.Message));
        }

        [HttpPost("v1/import-symbols-from-ssi")]
        public async Task<IActionResult> ImportSymbolsFromSsi(CancellationToken cancellationToken = default)
        {
            var command = new ImportSymbolsFromSsiCommand();
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(ApiResponse<int>.SuccessResponse(result.ImportedCount, result.Message));
        }
    }
}
