using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbols;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    [Route("api/symbol/v1")]
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
        public async Task<ActionResult<ApiResponse<GetSymbolsQueryResult>>> GetSymbols([FromQuery] GetSymbolsQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<GetSymbolsQueryResult>.SuccessResponse(result, result.Message));
        }
    }
}
