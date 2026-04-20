using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.UseCases.Notifications.Commands.RegisterDeviceToken;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Manages FCM push notification tokens for authenticated users.
    /// </summary>
    [Route("api/v1/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Registers or refreshes the Expo push token for the current user's device.
        /// Safe to call on every app launch (upsert semantics).
        /// </summary>
        /// <param name="command">The command containing the Expo push token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Success response when the token is stored.</returns>
        [HttpPost("register-token")]
        public async Task<ActionResult<ApiResponse>> RegisterToken(
            [FromBody] RegisterDeviceTokenCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
    }
}
