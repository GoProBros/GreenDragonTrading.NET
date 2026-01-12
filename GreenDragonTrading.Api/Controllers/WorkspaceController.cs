using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Workspace.Commands.CreateWorkspace;
using GreenDragonTrading.Application.UseCases.Workspace.Commands.UpdateWorkspace;
using GreenDragonTrading.Application.UseCases.Workspace.Queries.GetMyWorkspace;
using GreenDragonTrading.Application.UseCases.Workspace.Queries.GetWorkspaceByShareCode;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Workspace management endpoints
    /// </summary>
    [Route("api/v1/workspace")]
    [ApiController]
    public class WorkspaceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WorkspaceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get current user's workspaces
        /// </summary>
        [HttpGet("my-workspaces")]
        public async Task<ActionResult<ApiResponse<List<WorkspaceDto>>>> GetMyWorkspaces(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetMyWorkspaceQuery(), cancellationToken);
            return result;
        }

        /// <summary>
        /// Get workspace by share code
        /// </summary>
        [HttpGet("share/{shareCode}")]
        public async Task<ActionResult<ApiResponse<WorkspaceDto>>> GetWorkspaceByShareCode(
            [FromRoute] string shareCode,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetWorkspaceByShareCodeQuery(shareCode), cancellationToken);
            return result;
        }

        /// <summary>
        /// Create a new workspace layout
        /// </summary>
        /// <param name="command">Workspace creation data</param>
        /// <param name="cancellationToken"></param>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<WorkspaceDto>>> CreateWorkspace(
            [FromBody] CreateWorkspaceCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }

        /// <summary>
        /// Update an existing workspace layout
        /// </summary>
        /// <param name="id">Workspace ID</param>
        /// <param name="command">Update data</param>
        /// <param name="cancellationToken"></param>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<WorkspaceDto>>> UpdateWorkspace(
            [FromRoute] int id,
            [FromBody] UpdateWorkspaceCommand command,
            CancellationToken cancellationToken)
        {
            if(id != command.WorkspaceId)
            {
                return BadRequest(ApiResponse<WorkspaceDto>.Failure("ID không khớp."));
            }
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }
    }
}
