using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSectorsFromSsi;
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
        public async Task<IActionResult> ImportFromVnd(CancellationToken cancellationToken = default)
        {
            var command = new ImportSectorFromSsiCommand();
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(ApiResponse<int>.SuccessResponse(result.ImportedCount, result.Message));
        }
    }
}
