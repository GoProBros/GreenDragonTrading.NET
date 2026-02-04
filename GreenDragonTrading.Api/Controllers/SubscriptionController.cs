using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Subscriptions.Commands.CreateSubscription;
using GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetMySubscription;
using GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetSubscriptions;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    [Route("api/v1/subscriptions")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubscriptionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all available subscription packages
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<SubscriptionDto>>>> GetSubscriptions(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSubscriptionsQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get current user's subscription information
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserSubscriptionDto>>> GetMySubscription(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetMySubscriptionQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Create a new subscription package (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<ApiResponse<SubscriptionDto>>> CreateSubscription(
            [FromBody] CreateSubscriptionRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateSubscriptionCommand(
                request.Name,
                request.LevelOrder,
                request.MaxWorkspaces,
                request.Price,
                request.DurationInDays,
                request.AllowedModules
            );
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
    }
}
