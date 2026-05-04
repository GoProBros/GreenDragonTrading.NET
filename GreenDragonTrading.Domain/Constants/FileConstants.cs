namespace GreenDragonTrading.Domain.Constants;

/// <summary>
/// File-related constants
/// </summary>
public static class FileConstants
{
    /// <summary>
    /// Base directory for local file storage (relative to application root)
    /// </summary>
    public const string BASE_STORAGE_PATH = "LocalStorage/Files";

    /// <summary>
    /// Maximum file size in bytes (50MB)
    /// </summary>
    public const long MAX_FILE_SIZE = 52428800; // 50 * 1024 * 1024

    /// <summary>
    /// Subdirectory for financial reports
    /// </summary>
    public const string FINANCIAL_REPORTS_DIR = "FinancialReports";

    /// <summary>
    /// Subdirectory for analysis reports
    /// </summary>
    public const string ANALYSIS_REPORTS_DIR = "AnalysisReports";

    /// <summary>
    /// Subdirectory for avatars
    /// </summary>
    public const string AVATARS_DIR = "Avatars";

    /// <summary>
    /// Subdirectory for company logos
    /// </summary>
    public const string COMPANY_LOGOS_DIR = "CompanyLogos";

    /// <summary>
    /// Allowed file extensions
    /// </summary>
    public static class AllowedExtensions
    {
        public static readonly string[] Documents = { ".pdf", ".docx" };
        public static readonly string[] Images = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        public static readonly string[] All = Documents.Concat(Images).ToArray();
    }

    /// <summary>
    /// MIME types mapping
    /// </summary>
    public static class MimeTypes
    {
        public const string Pdf = "application/pdf";
        public const string Docx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public const string Jpeg = "image/jpeg";
        public const string Png = "image/png";
        public const string Gif = "image/gif";
        public const string WebP = "image/webp";
        public const string OctetStream = "application/octet-stream";

        public static string GetMimeType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => Pdf,
                ".docx" => Docx,
                ".jpg" or ".jpeg" => Jpeg,
                ".png" => Png,
                ".gif" => Gif,
                ".webp" => WebP,
                _ => OctetStream
            };
        }
    }
}
