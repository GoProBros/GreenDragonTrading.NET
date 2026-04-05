using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Alerts.Commands.CreateAlert;
using GreenDragonTrading.Application.UseCases.Alerts.Commands.DeleteAlert;
using GreenDragonTrading.Application.UseCases.Alerts.Events;
using GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertsByUserId;
using GreenDragonTrading.Domain.Constants.SSI;
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
        /// Alert types: 1 = price, 2 = volume. Condition types: 1 = above, 2 = below, 3 = increase change by %, 4 = decrease change by %. Notification channels: 1 = System chat, 2 = Chat message, 3 = Telegram.
        /// Name là tên alert do user đặt, chưa biết dùng để làm gì. thèn thịnh vẽ DB :)))
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

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse>> DeleteAlert(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteAlertCommand(id), cancellationToken);
            return result;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<AlertDto>>>> GetAlertsByUserId(
            [FromQuery] Guid userId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAlertsByUserIdQuery(userId), cancellationToken);
            return result;
        }
    }
}
