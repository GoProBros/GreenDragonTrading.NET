using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Subscriptions.Commands.CreateSubscription;
using GreenDragonTrading.Application.UseCases.Subscriptions.Commands.UpdateSubscriptionPrice;
using GreenDragonTrading.Application.UseCases.Subscriptions.Commands.UpdateSubscriptionStatus;
using GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetCustomerRetentionStatistics;
using GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetMySubscription;
using GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetSubscriptionStatistics;
using GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetSubscriptions;
using GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetWatchListTopInterestedSymbols;
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
        /// Get user/subscription/revenue statistics for admin dashboard.
        /// Monthly metrics are calculated for the latest rolling 12 months from current time.
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Staff)}")]
        public async Task<ActionResult<ApiResponse<SubscriptionStatisticsDto>>> GetStatistics(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSubscriptionStatisticsQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get customer retention statistics based on number of package registrations.
        /// </summary>
        [HttpGet("statistics/customer-retention")]
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Staff)}")]
        public async Task<ActionResult<ApiResponse<CustomerRetentionStatisticsDto>>> GetCustomerRetentionStatistics(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCustomerRetentionStatisticsQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get top interested symbols from active users' watch lists.
        /// </summary>
        [HttpGet("statistics/watchlist-top-symbols")]
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Staff)}")]
        public async Task<ActionResult<ApiResponse<WatchListTopInterestedSymbolsDto>>> GetWatchListTopInterestedSymbols(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetWatchListTopInterestedSymbolsQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Create a new subscription package (Admin/Staff)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Staff)}")]
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
                request.AllowedModules,
                request.IsActive
            );
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Toggles active/inactive status of a subscription package (Admin/Staff)
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Staff)}")]
        public async Task<ActionResult<ApiResponse<SubscriptionDto>>> UpdateSubscriptionStatus(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            var command = new UpdateSubscriptionStatusCommand(id);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Updates subscription fields (price and/or allowed modules) (Admin/Staff)
        /// </summary>
        [HttpPatch("{id}")]
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Staff)}")]
        public async Task<ActionResult<ApiResponse<SubscriptionDto>>> UpdateSubscription(
            [FromRoute] int id,
            [FromBody] UpdateSubscriptionPriceRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateSubscriptionPriceCommand(id, request.Price, request.AllowedModules);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
    }
}
