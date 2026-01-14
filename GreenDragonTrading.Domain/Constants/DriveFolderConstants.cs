namespace GreenDragonTrading.Domain.Constants
{
    /// <summary>
    /// Google Drive folder types for organizing different file categories
    /// </summary>
    public static class DriveFolderConstants
    {
        /// <summary>
        /// Báo cáo tài chính (Financial Reports)
        /// </summary>
        public const string FINANCIAL_REPORTS = "FinancialReports";

        /// <summary>
        /// Logo công ty (Company Logos)
        /// </summary>
        public const string COMPANY_LOGOS = "CompanyLogos";

        /// <summary>
        /// Tài liệu người dùng (User Documents)
        /// </summary>
        public const string USER_DOCUMENTS = "UserDocuments";

        /// <summary>
        /// File export (Exported Files)
        /// </summary>
        public const string EXPORTS = "Exports";

        /// <summary>
        /// Tài liệu đính kèm (Attachments)
        /// </summary>
        public const string ATTACHMENTS = "Attachments";

        /// <summary>
        /// Hình ảnh (Images)
        /// </summary>
        public const string IMAGES = "Images";

        /// <summary>
        /// Template files
        /// </summary>
        public const string TEMPLATES = "Templates";

        /// <summary>
        /// Backup files
        /// </summary>
        public const string BACKUPS = "Backups";

        /// <summary>
        /// Get all folder types
        /// </summary>
        public static readonly string[] AllFolderTypes = new[]
        {
            FINANCIAL_REPORTS,
            COMPANY_LOGOS,
            USER_DOCUMENTS,
            EXPORTS,
            ATTACHMENTS,
            IMAGES,
            TEMPLATES,
            BACKUPS
        };
    }
}
