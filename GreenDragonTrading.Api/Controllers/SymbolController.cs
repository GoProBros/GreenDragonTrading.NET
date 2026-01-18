using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Symbols.Commands.UpdateSymbolSector;
using GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbol;
using GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbols;
using GreenDragonTrading.Application.UseCases.Symbols.Queries.SearchSymbols;
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
        /// Updates a symbol's sector. Only level 4 sectors can be assigned.
        /// </summary>
        /// <param name="command">Command containing the new sector ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated symbol details</returns>
        [HttpPatch("sector-change")]
        public async Task<ActionResult<ApiResponse<SymbolDto>>> UpdateSymbolSector(
            [FromBody] UpdateSymbolSectorCommand command,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
