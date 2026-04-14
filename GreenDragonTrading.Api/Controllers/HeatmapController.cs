using GreenDragonTrading.Application.UseCases.Heatmap.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers;

/// <summary>
/// Controller xử lý các API liên quan đến heatmap
/// </summary>
[ApiController]
[Route("api/v1/heatmap")]
public class HeatmapController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HeatmapController> _logger;

    public HeatmapController(
        IMediator mediator,
        ILogger<HeatmapController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lấy dữ liệu heatmap cho thị trường
    /// </summary>
    /// <param name="exchange">Mã sàn (HSX, HNX, UPCOM) - optional</param>
    /// <param name="sector">Mã ngành - optional</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dữ liệu heatmap</returns>
    /// <response code="200">Trả về dữ liệu heatmap thành công</response>
    /// <response code="500">Lỗi server khi xử lý request</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHeatmapData(
        [FromQuery] string? exchange,
        [FromQuery] string? sector,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /api/heatmap called with exchange={Exchange}, sector={Sector}",
                exchange, sector);

            var query = new GetHeatmapDataQuery
            {
                Exchange = exchange,
                Sector = sector
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetHeatmapData endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Có lỗi xảy ra khi lấy dữ liệu heatmap" });
        }
    }
}
