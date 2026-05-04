using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Alerts.Commands.CreateAlert;
using GreenDragonTrading.Application.UseCases.Alerts.Commands.CreateAlertTemplate;
using GreenDragonTrading.Application.UseCases.Alerts.Commands.ToggleAlertTemplateStatus;
using GreenDragonTrading.Application.UseCases.Alerts.Commands.ToggleAlertStatus;
using UpdateAlertTemplate = GreenDragonTrading.Application.UseCases.Alerts.Commands.UpdateAlertTemplate;
using GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertById;
using GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertTemplateById;
using GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertsByUserId;
using GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertTemplates;
using GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertTemplatePlaceholders;
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

        /// <summary>
        /// Creates a new alert template (admin/staff only).
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("templates")]
        public async Task<ActionResult<ApiResponse<AlertTemplateDto>>> CreateAlertTemplate(
            [FromBody] CreateAlertTemplateCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }

        /// <summary>
        /// Returns alert templates (admin/staff only).
        /// </summary>
        [HttpGet("templates")]
        public async Task<ActionResult<ApiResponse<List<AlertTemplateDto>>>> GetAlertTemplates(
            [FromQuery] GetAlertTemplatesQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return result;
        }

        /// <summary>
        /// Returns a single alert template by id (admin/staff only).
        /// </summary>
        [HttpGet("templates/{id:int}")]
        public async Task<ActionResult<ApiResponse<AlertTemplateDto>>> GetAlertTemplateById(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAlertTemplateByIdQuery(id), cancellationToken);
            return result;
        }

        /// <summary>
        /// Updates an alert template (admin/staff only).
        /// </summary>
        [HttpPut("templates/{id:int}")]
        public async Task<ActionResult<ApiResponse<AlertTemplateDto>>> UpdateAlertTemplate(
            [FromRoute] int id,
            [FromBody] UpdateAlertTemplate.UpdateAlertTemplateCommand command,
            CancellationToken cancellationToken)
        {
            var normalized = command with { Id = id };
            var result = await _mediator.Send(normalized, cancellationToken);
            return result;
        }

        /// <summary>
        /// Toggles alert template status (admin/staff only).
        /// </summary>
        [HttpPatch("templates/{id:int}/status")]
        public async Task<ActionResult<ApiResponse<AlertTemplateDto>>> ToggleAlertTemplateStatus(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ToggleAlertTemplateStatusCommand(id), cancellationToken);
            return result;
        }

        /// <summary>
        /// Returns allowed placeholders for alert templates.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("placeholders")]
        public async Task<ActionResult<ApiResponse<AlertTemplatePlaceholdersDto>>> GetAlertTemplatePlaceholders(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAlertTemplatePlaceholdersQuery(), cancellationToken);
            return result;
        }
    }
}
