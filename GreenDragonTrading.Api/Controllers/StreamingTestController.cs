using GreenDragonTrading.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Controller for testing SSI streaming service.
    /// </summary>
    [Route("api/streaming")]
    [ApiController]
    public class StreamingTestController : ControllerBase
    {
        private readonly ISsiStreamingService _streamingService;
        private readonly ILogger<StreamingTestController> _logger;

        public StreamingTestController(
            ISsiStreamingService streamingService,
            ILogger<StreamingTestController> logger)
        {
            _streamingService = streamingService;
            _logger = logger;
        }

        /// <summary>
        /// Starts the SSI streaming connection.
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> Start(CancellationToken cancellationToken)
        {
            try
            {
                await _streamingService.StartAsync(cancellationToken);
                return Ok(new { message = "Streaming started successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start streaming.");
                return StatusCode(500, new { message = "Failed to start streaming.", error = ex.Message });
            }
        }

        /// <summary>
        /// Stops the SSI streaming connection.
        /// </summary>
        [HttpPost("stop")]
        public IActionResult Stop()
        {
            try
            {
                _streamingService.Stop();
                return Ok(new { message = "Streaming stopped successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop streaming.");
                return StatusCode(500, new { message = "Failed to stop streaming.", error = ex.Message });
            }
        }

        /// <summary>
        /// Subscribes to a channel with filter condition.
        /// Example filter: "X-TRADE:SSI" or "X-QUOTE:VNM,FPT"
        /// </summary>
        /// <param name="filter">The filter condition for channel subscription.</param>
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromQuery] string filter, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return BadRequest(new { message = "Filter condition is required." });
            }

            try
            {
                await _streamingService.SwitchChannelsAsync(filter);
                return Ok(new { message = $"Subscribed to channel: {filter}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to subscribe to channel.");
                return StatusCode(500, new { message = "Failed to subscribe.", error = ex.Message });
            }
        }
    }
}
