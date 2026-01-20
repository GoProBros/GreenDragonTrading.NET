using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.UseCases.Sectors.Commands.CreateSector;
using GreenDragonTrading.Application.UseCases.Sectors.Commands.DeleteSector;
using GreenDragonTrading.Application.UseCases.Sectors.Commands.UpdateSector;
using GreenDragonTrading.Application.UseCases.Sectors.Queries.GetSectorById;
using GreenDragonTrading.Application.UseCases.Sectors.Queries.GetSectors;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static GreenDragonTrading.Application.DTOs.SectorDtos;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Controller for managing sector-related operations.
    /// </summary>
    [Route("api/v1/sectors")]
    [ApiController]
    public class SectorController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Retrieves a paginated list of sectors with their associated symbols.
        /// </summary>
        /// <param name="level">Sector level (1-4). If null, retrieves all levels. Commonly used: Level 2</param>
        /// <param name="status">Sector status filter. 0=InActive, 1=Active. If null, retrieves only active sectors by default</param>
        /// <param name="pageIndex">Current page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of sectors with associated symbols</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<SectorDto>>>> GetSectors(
            [FromQuery] int? level = null,
            [FromQuery] CommonStatus? status = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetSectorsQuery(level, status)
            {
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a single sector by its ID with associated symbols.
        /// </summary>
        /// <param name="id">Sector ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Sector details with associated symbols</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SectorDto>>> GetSectorById(
            string id,
            CancellationToken cancellationToken)
        {
            var query = new GetSectorByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new sector.
        /// </summary>
        /// <param name="command">Sector creation details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created sector details</returns>
        /// <remarks>
        /// Level 1 sectors must not have a parent. Levels 2-4 must have a valid parent sector.
        /// Parent sector level must be exactly one level above the new sector.
        /// </remarks>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<SectorDto>>> CreateSector(
            [FromBody] CreateSectorCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetSectorById), new { id = command.Id }, result);
        }

        /// <summary>
        /// Updates an existing sector's names.
        /// </summary>
        /// <param name="id">Sector ID</param>
        /// <param name="command">Sector update details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated sector details</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<SectorDto>>> UpdateSector(
            string id,
            [FromBody] UpdateSectorCommand command,
            CancellationToken cancellationToken)
        {
            if (id != command.Id)
            {
                return BadRequest(ApiResponse<SectorDto>.Failure("ID mismatch"));
            }

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Soft deletes a sector (sets status to Inactive).
        /// /// Cannot delete sectors that have active child sectors.
        /// </summary>
        /// <param name="id">Sector ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success confirmation</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteSector(
            string id,
            CancellationToken cancellationToken)
        {
            var command = new DeleteSectorCommand(id);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
    }
}
