using GreenDragonTrading.Infrastructure.Hubs;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <inheritdoc/>
    public class MarketDataBroadcaster(
        IHubContext<MarketDataHub> hubContext,
        ILogger<MarketDataBroadcaster> logger) : IMarketDataBroadcaster
    {
        private readonly IHubContext<MarketDataHub> _hubContext = hubContext;
        private readonly ILogger<MarketDataBroadcaster> _logger = logger;

        /// <inheritdoc/>
        public async Task BroadcastMarketDataAsync<T>(string symbol, T data, CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients
                    .Group(symbol)
                    .SendAsync("ReceiveMarketData", data, cancellationToken);

                _logger.LogDebug("Broadcasted market data for symbol: {Symbol}", symbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting market data for symbol: {Symbol}", symbol);
                throw;
            }
        }
    }
}
