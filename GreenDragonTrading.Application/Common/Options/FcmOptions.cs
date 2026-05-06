namespace GreenDragonTrading.Application.Common.Options
{
    /// <summary>
    /// Configuration for FCM HTTP v1 push notification delivery.
    /// Provide either <see cref="ServiceAccountJson"/> (inline JSON string, e.g. from env var)
    /// or <see cref="ServiceAccountKeyPath"/> (path to the downloaded JSON file).
    /// </summary>
    public class FcmOptions
    {
        public const string SectionName = "FcmOptions";

        /// <summary>Firebase project ID (e.g. "kf-stock").</summary>
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Inline service account JSON content.
        /// Useful when injected via environment variable / secret manager.
        /// If set, takes precedence over <see cref="ServiceAccountKeyPath"/>.
        /// </summary>
        public string? ServiceAccountJson { get; set; }

        /// <summary>
        /// Absolute or relative path to the Firebase service account JSON file.
        /// Download from Firebase Console → Project Settings → Service Accounts → Generate new private key.
        /// </summary>
        public string? ServiceAccountKeyPath { get; set; }
    }
}
