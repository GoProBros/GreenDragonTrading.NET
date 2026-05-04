using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Notifications.Commands.RegisterDeviceToken
{
    /// <summary>
    /// Registers (or refreshes) an Expo push token for the authenticated user's current device.
    /// Safe to call on every app launch — uses upsert semantics.
    /// </summary>
    public class RegisterDeviceTokenCommand : IRequest<ApiResponse>
    {
        /// <summary>Expo push token in the format ExponentPushToken[xxxxxxxxxxxxxxxx].</summary>
        public string ExpoPushToken { get; set; } = string.Empty;
    }
}
