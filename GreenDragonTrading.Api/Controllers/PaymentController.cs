using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Payments.Commands.CancelPaymentLink;
using GreenDragonTrading.Application.UseCases.Payments.Commands.CreatePaymentLink;
using GreenDragonTrading.Application.UseCases.Payments.Commands.ProcessMomoIpn;
using GreenDragonTrading.Application.UseCases.Payments.Commands.ProcessPayOSWebhook;
using GreenDragonTrading.Application.UseCases.Payments.Commands.SyncMomoPayment;
using GreenDragonTrading.Application.UseCases.Payments.Queries.GetMyTransactions;
using GreenDragonTrading.Application.UseCases.Payments.Queries.GetPaymentStatus;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;

namespace GreenDragonTrading.Api.Controllers
{
    [Route("api/v1/payments")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a payment link for the authenticated user to purchase a subscription.
        /// The user selects the payment provider via <c>paymentProvider</c>: 1 = PayOS, 2 = Momo.
        /// </summary>
        [HttpPost("create-link")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PaymentLinkResponse>>> CreatePaymentLink(
            [FromBody] CreatePaymentLinkRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreatePaymentLinkCommand(request.SubscriptionId, request.PaymentProvider);
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse>> ProcessWebhook(
            [FromBody] WebhookType webhookBody,
            CancellationToken cancellationToken)
        {
            try
            {
                if (webhookBody.data == null || webhookBody.desc == "test")
                {
                    return Ok(new { Success = true, Message = "Webhook URL is active" });
                }

                var command = new ProcessPayOSWebhookCommand(webhookBody);
                var result = await _mediator.Send(command, cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Webhook Error]: {ex.Message}");
                return Ok(new { Success = false, Message = "Internal error handled" });
            }
        }

        [HttpGet("status/{orderCode}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PaymentStatusResponse>>> GetPaymentStatus(
            [FromRoute] long orderCode,
            CancellationToken cancellationToken)
        {
            var query = new GetPaymentStatusQuery(orderCode);
            var result = await _mediator.Send(query, cancellationToken);
            return result;
        }

        /// <summary>
        /// Gets paginated transaction history of current authenticated user.
        /// Supports optional filters by transaction status and payment provider.
        /// paymentProvider filter values: 1 = PayOS, 2 = Momo. status filter values: 0 = Pending, 1 = Completed, 2 = Cancelled, 3 = Expired.
        /// </summary>
        [HttpGet("me/transactions")]
        [Authorize(Roles = nameof(UserRole.User))]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<PaymentTransactionDto>>>> GetMyTransactions(
            [FromQuery] GetMyTransactionsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("cancel/{orderCode}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PaymentInformationResponse>>> CancelPayment(
            long orderCode,
            CancellationToken cancellationToken,
            [FromBody] string reason = "User cancel")
        {
            var command = new CancelPaymentLinkCommand(orderCode, reason);
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }

        /// <summary>
        /// Receives Instant Payment Notifications (IPN) from Momo after a transaction completes.
        /// Momo calls this endpoint with the transaction result. Must be publicly accessible.
        /// </summary>
        [HttpPost("momo/ipn")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse>> ProcessMomoIpn(
            [FromBody] MomoIpnRequest ipnRequest,
            CancellationToken cancellationToken)
        {
            try
            {
                var command = new ProcessMomoIpnCommand(ipnRequest);
                var result = await _mediator.Send(command, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Momo IPN Error]: {ex.Message}");
                return Ok(new { Success = false, Message = "Internal error handled" });
            }
        }

        /// <summary>
        /// Manually syncs payment status by order code.
        /// If payment is confirmed, subscription data is updated in DB.
        /// If payment has timed out and is auto-cancelled by provider, transaction status is also updated in DB.
        /// </summary>
        /// <param name="orderCode">The order code returned when the payment link was created.</param>
        /// <param name="cancellationToken"></param>
        [HttpPost("sync/{orderCode}")]
        public async Task<ActionResult<ApiResponse<WebhookUpdateResult>>> SyncPayment(
            [FromRoute] long orderCode,
            CancellationToken cancellationToken)
        {
            var command = new SyncMomoPaymentCommand(orderCode);
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }
    }
}
