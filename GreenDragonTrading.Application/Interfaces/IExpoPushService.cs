namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Sends push notifications via FCM HTTP v1 API directly,
    /// bypassing Expo Push Service so that standalone APKs receive notifications
    /// without depending on Expo Go's Firebase sender ID.
    /// </summary>
    public interface IExpoPushService
    {
        /// <summary>
        /// Sends push notifications to the given FCM device tokens.
        /// Batches automatically (max 100 per request).
        /// </summary>
        /// <param name="tokens">List of raw FCM device tokens (from getDevicePushTokenAsync on mobile).</param>
        /// <param name="title">Notification title shown in OS tray.</param>
        /// <param name="body">Notification body text.</param>
        /// <param name="dataUrl">
        /// Optional deep-link URL placed in notification data (e.g. "kafistock://notifications").
        /// Mobile app reads data.url on tap and navigates via Linking.openURL.
        /// </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of stale tokens that should be removed (FCM reported UNREGISTERED).</returns>
        Task<IReadOnlyList<string>> SendAsync(
            IReadOnlyList<string> tokens,
            string title,
            string body,
            string? dataUrl = null,
            CancellationToken cancellationToken = default);
    }
}
