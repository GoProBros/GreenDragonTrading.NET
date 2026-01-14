namespace GreenDragonTrading.Application.Common.Options
{
    /// <summary>
    /// Configuration options for Google Drive API integration
    /// </summary>
    public class GoogleDriveOptions
    {
        public const string SectionName = "GoogleDrive";

        /// <summary>
        /// Path to the service account credentials JSON file (optional if using JsonCredentials)
        /// </summary>
        public string? ServiceAccountKeyPath { get; set; }

        /// <summary>
        /// Service account credentials as JSON string from environment variable (recommended)
        /// </summary>
        public string? JsonCredentials { get; set; }

        /// <summary>
        /// Application name for Google Drive API
        /// </summary>
        public string ApplicationName { get; set; } = "GreenDragonTrading";

        /// <summary>
        /// Root folder ID on Google Drive for all application files
        /// </summary>
        public string RootFolderId { get; set; } = null!;

        /// <summary>
        /// Folder structure configuration - folder IDs for different file types
        /// </summary>
        public Dictionary<string, string> Folders { get; set; } = new();

        /// <summary>
        /// Enable/disable Google Drive integration
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Maximum file size in bytes (default 50MB)
        /// </summary>
        public long MaxFileSizeBytes { get; set; } = 52428800; // 50MB

        /// <summary>
        /// Auto-create folders if they don't exist
        /// </summary>
        public bool AutoCreateFolders { get; set; } = true;
    }
}
