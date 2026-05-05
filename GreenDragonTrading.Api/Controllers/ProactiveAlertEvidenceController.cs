using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Admin controller for proactive alert evidence.
    /// All endpoints are publicly accessible (no auth) for demo purposes.
    /// </summary>
    [ApiController]
    [Route("api/admin/proactive-evidence")]
    public class ProactiveAlertEvidenceController : ControllerBase
    {
        private readonly IProactiveAlertEvidenceService _evidenceService;

        public ProactiveAlertEvidenceController(IProactiveAlertEvidenceService evidenceService)
        {
            _evidenceService = evidenceService;
        }

        /// <summary>
        /// Returns aggregate statistics for the evidence dashboard.
        /// </summary>
        [HttpGet("stats")]
        [Produces("application/json")]
        public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
        {
            var stats = await _evidenceService.GetStatsAsync(cancellationToken);
            return Ok(ApiResponse<ProactiveAlertEvidenceStatsDto>.Success(stats, "Lấy thống kê thành công"));
        }

        /// <summary>
        /// Lists trace summaries with optional filters.
        /// </summary>
        [HttpGet("traces")]
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
        [HttpGet("traces/{traceId}")]
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

    }
}
