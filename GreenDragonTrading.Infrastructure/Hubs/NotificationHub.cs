using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.Hubs
{
    /// <summary>
    /// Dedicated SignalR hub for notification and chat message events.
    /// Clients only need to connect and listen for server-pushed events.
    /// </summary>
    [Authorize]
    public class NotificationHub(ILogger<NotificationHub> logger) : Hub
    {
        private readonly ILogger<NotificationHub> _logger = logger;

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation(
                "NotificationHub connected: ConnectionId={ConnectionId}, UserId={UserId}",
                Context.ConnectionId,
                Context.UserIdentifier ?? "unknown");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception == null)
            {
                _logger.LogInformation(
                    "NotificationHub disconnected: ConnectionId={ConnectionId}, UserId={UserId}",
                    Context.ConnectionId,
                    Context.UserIdentifier ?? "unknown");
            }
            else
            {
                _logger.LogWarning(
                    exception,
                    "NotificationHub disconnected with error: ConnectionId={ConnectionId}, UserId={UserId}",
                    Context.ConnectionId,
                    Context.UserIdentifier ?? "unknown");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}