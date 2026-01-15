namespace GreenDragonTrading.Domain.Constants
{
    /// <summary>
    /// File storage folder structure constants
    /// </summary>
    public static class FileStorageConstants
    {
        /// <summary>
        /// Base folder cho financial reports
        /// Format: financial-reports/{year}/{ticker}/{filename}
        /// </summary>
        public const string FinancialReportsFolder = "financial-reports";

        /// <summary>
        /// Helper method to build financial report path
        /// </summary>
        public static string GetFinancialReportPath(int year, string ticker)
        {
            return $"{FinancialReportsFolder}/{year}/{ticker}";
        }
    }
}
