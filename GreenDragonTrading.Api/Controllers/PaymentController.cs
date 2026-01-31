using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Payments.Commands.CancelPaymentLink;
using GreenDragonTrading.Application.UseCases.Payments.Commands.CreatePaymentLink;
using GreenDragonTrading.Application.UseCases.Payments.Commands.ProcessPayOSWebhook;
using GreenDragonTrading.Application.UseCases.Payments.Queries.GetPaymentStatus;
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

        [HttpPost("create-link")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PaymentLinkResponse>>> CreatePaymentLink(
            [FromBody] CreatePaymentLinkRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreatePaymentLinkCommand(request.SubscriptionId);
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

        [HttpPost("cancel/{orderCode}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PaymentInformationResponse>>> CancelPayment(
            long orderCode,
            CancellationToken cancellationToken,
            [FromBody] string reason = "User cancel")
        {
            var command = new CancelPaymentLinkCommand(orderCode, reason);
            var result = await _mediator.Send(command,cancellationToken);
            return result;
        }
    }
}
