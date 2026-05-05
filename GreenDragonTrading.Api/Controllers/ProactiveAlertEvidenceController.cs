using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.UseCases.Alerts.Commands.UpdateProactiveAlertLayerBSettings;
using GreenDragonTrading.Application.UseCases.Alerts.Queries.GetProactiveAlertLayerBSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Proactive alert controller for evidence and settings endpoints.
    /// Evidence endpoints are public for demo purposes.
    /// </summary>
    [ApiController]
    [Route("api/v1/proactive-alert")]
    public class ProactiveAlertController : ControllerBase
    {
        private readonly IProactiveAlertEvidenceService _evidenceService;
        private readonly IMediator _mediator;

        public ProactiveAlertController(
            IProactiveAlertEvidenceService evidenceService,
            IMediator mediator)
        {
            _evidenceService = evidenceService;
            _mediator = mediator;
        }

        /// <summary>
        /// Returns aggregate statistics for the evidence dashboard.
        /// </summary>
        [HttpGet("evidence/stats")]
        [Produces("application/json")]
        public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
        {
            var stats = await _evidenceService.GetStatsAsync(cancellationToken);
            return Ok(ApiResponse<ProactiveAlertEvidenceStatsDto>.Success(stats, "Lấy thống kê thành công"));
        }

        /// <summary>
        /// Lists trace summaries with optional filters.
        /// </summary>
        [HttpGet("evidence/traces")]
        [Produces("application/json")]
        public async Task<IActionResult> ListTraces(
            [FromQuery] string? ticker,
            [FromQuery] string? finalResult,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var query = new ProactiveAlertTraceQueryDto
            {
                Ticker = ticker,
                FinalResult = finalResult,
                From = from,
                To = to,
                PageIndex = pageIndex,
                PageSize = pageSize,
            };

            var (items, totalCount) = await _evidenceService.ListTracesAsync(query, cancellationToken);
            var paged = PaginatedResponse<ProactiveAlertTraceSummaryDto>.Create(items, totalCount, pageIndex + 1, pageSize);
            return Ok(ApiResponse<PaginatedResponse<ProactiveAlertTraceSummaryDto>>.Success(paged, "Lấy danh sách thành công"));
        }

        /// <summary>
        /// Retrieves full detail for a single trace.
        /// </summary>
        [HttpGet("evidence/traces/{traceId}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetTrace(string traceId, CancellationToken cancellationToken)
        {
            var trace = await _evidenceService.GetTraceAsync(traceId, cancellationToken);
            if (trace == null)
            {
                return NotFound(ApiResponse.Failure($"Không tìm thấy trace {traceId}"));
            }

            return Ok(ApiResponse<ProactiveAlertTraceDto>.Success(trace, "Lấy trace thành công"));
        }

        /// <summary>
        /// Gets the current global layer B settings.
        /// </summary>
        [HttpGet("settings/layer-b")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<ApiResponse<ProactiveAlertLayerBSettingsDto>>> GetLayerBSettings(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProactiveAlertLayerBSettingsQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Updates the current global layer B settings.
        /// </summary>
        [HttpPut("settings/layer-b")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<ApiResponse<ProactiveAlertLayerBSettingsDto>>> UpdateLayerBSettings(
            [FromBody] UpdateProactiveAlertLayerBSettingsCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

    }
}
