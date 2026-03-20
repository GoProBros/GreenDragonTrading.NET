using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace GreenDragonTrading.Infrastructure.Hubs
{
    /// <summary>
    /// Resolves SignalR user id from JWT claims for Clients.User targeting.
    /// </summary>
    public class MarketDataUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var principal = connection.User;
            if (principal == null)
            {
                return null;
            }

            return principal.FindFirstValue("uid")
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue("sub");
        }
    }
}
