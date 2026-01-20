using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.Services;

/// <summary>
/// Background service to periodically broadcast heatmap updates to all subscribed clients
/// </summary>
public class HeatmapBroadcastService : BackgroundService
{
    private readonly ILogger<HeatmapBroadcastService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<MarketDataHub> _hubContext;
    
    // Broadcast interval: 30 seconds
    private readonly TimeSpan _broadcastInterval = TimeSpan.FromSeconds(30);

    public HeatmapBroadcastService(
        ILogger<HeatmapBroadcastService> logger,
        IServiceProvider serviceProvider,
        IHubContext<MarketDataHub> hubContext)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔥 HeatmapBroadcastService starting...");

        // Wait 10 seconds before first broadcast to allow system initialization
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await BroadcastHeatmapUpdatesAsync(stoppingToken);
                
                // Wait for next broadcast interval
                await Task.Delay(_broadcastInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("🛑 HeatmapBroadcastService stopping...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in HeatmapBroadcastService main loop");
                
                // Wait a bit before retry on error
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
        
        _logger.LogInformation("🛑 HeatmapBroadcastService stopped");
    }

    /// <summary>
    /// Broadcast heatmap updates to all subscribed groups
    /// </summary>
    private async Task BroadcastHeatmapUpdatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Use scoped service to avoid threading issues
            using var scope = _serviceProvider.CreateScope();
            var heatmapService = scope.ServiceProvider.GetRequiredService<IHeatmapService>();

            // Define groups to broadcast
            var broadcastGroups = new[]
            {
                new { Exchange = (string?)null, Sector = (string?)null, GroupName = "HEATMAP:ALL" },
                new { Exchange = "HSX", Sector = (string?)null, GroupName = "HEATMAP:HSX" },
                new { Exchange = "HNX", Sector = (string?)null, GroupName = "HEATMAP:HNX" },
                new { Exchange = "UPCOM", Sector = (string?)null, GroupName = "HEATMAP:UPCOM" },
            };

            var broadcastTasks = new List<Task>();

            foreach (var group in broadcastGroups)
            {
                var task = BroadcastToGroupAsync(
                    heatmapService, 
                    group.Exchange, 
                    group.Sector, 
                    group.GroupName, 
                    cancellationToken);
                
                broadcastTasks.Add(task);
            }

            // Wait for all broadcasts to complete
            await Task.WhenAll(broadcastTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error broadcasting heatmap updates");
        }
    }

    /// <summary>
    /// Broadcast heatmap data to a specific SignalR group
    /// </summary>
    private async Task BroadcastToGroupAsync(
        IHeatmapService heatmapService,
        string? exchange,
        string? sector,
        string groupName,
        CancellationToken cancellationToken)
    {
        try
        {
            // Fetch heatmap data
            var heatmapData = await heatmapService.GetHeatmapDataAsync(
                exchange, 
                sector, 
                cancellationToken);

            if (heatmapData == null || !heatmapData.Items.Any())
            {
                _logger.LogDebug(
                    "No heatmap data for exchange={Exchange}, sector={Sector}",
                    exchange ?? "ALL", sector ?? "ALL");
                return;
            }

            // Broadcast to SignalR group
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync("ReceiveHeatmapData", heatmapData, cancellationToken);

            _logger.LogInformation(
                "📊 Broadcasted heatmap update to {GroupName}: {Count} items (Exchange={Exchange}, Sector={Sector})",
                groupName, heatmapData.TotalCount, exchange ?? "ALL", sector ?? "ALL");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Error broadcasting to {GroupName} (Exchange={Exchange}, Sector={Sector})",
                groupName, exchange ?? "ALL", sector ?? "ALL");
        }
    }
}
