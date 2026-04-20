namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Sends push notifications via the Expo Push API (FCM under the hood).
    /// Deliver notifications to devices even when the app is backgrounded or killed.
    /// </summary>
    public interface IExpoPushService
    {
        /// <summary>
        /// Sends push notifications to the given Expo push tokens.
        /// Batches automatically (max 100 per request).
        /// </summary>
        /// <param name="tokens">List of ExponentPushToken[...] strings.</param>
        /// <param name="title">Notification title shown in OS tray.</param>
        /// <param name="body">Notification body text.</param>
        /// <param name="dataUrl">
        /// Optional deep-link URL placed in notification data (e.g. "kafistock://notifications").
        /// Mobile app reads data.url on tap and navigates via Linking.openURL.
        /// </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of stale tokens that should be removed (Expo reported DeviceNotRegistered).</returns>
        Task<IReadOnlyList<string>> SendAsync(
            IReadOnlyList<string> tokens,
            string title,
            string body,
            string? dataUrl = null,
            CancellationToken cancellationToken = default);
    }
}
