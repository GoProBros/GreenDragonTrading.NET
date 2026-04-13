using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Alerts.Commands.CreateAlert;
using GreenDragonTrading.Application.UseCases.Alerts.Commands.ToggleAlertStatus;
using GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertById;
using GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertsByUserId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GreenDragonTrading.Api.Controllers
{
    [ApiController]
    [Route("api/v1/alerts")]
    [Authorize]
    public class AlertController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        public class SimulateSsiAlertRequest
        {
            public string DataType { get; set; } = string.Empty;
            public JsonElement Content { get; set; }
        }

        /// <summary>
        /// Creates an alert. Alert types: 1 = price, 2 = volume. Condition types: 1 = above, 2 = below, 3 = increase by percentage, 4 = decrease by percentage.
        /// For percentage conditions, current market value is read from Redis at request time.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<AlertDto>>> CreateAlert(
            [FromBody] CreateAlertCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }

        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<ApiResponse<AlertDto>>> ToggleAlertStatus(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ToggleAlertStatusCommand(id), cancellationToken);
            return result;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<AlertDto>>>> GetAlertsByUserId(
            [FromQuery] GetAlertsByUserIdQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return result;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<AlertDto>>> GetAlertById(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAlertByIdQuery(id), cancellationToken);
            return result;
        }
    }
}
