namespace GreenDragonTrading.Domain.Enums
{
    /// <summary>
    /// Financial Report Status
    /// Trạng thái báo cáo tài chính
    /// </summary>
    public enum FinancialReportStatus : short
    {
        /// <summary>
        /// Status : pending
        /// Chờ xử lý
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Status : processing
        /// Đang xử lý
        /// </summary>
        Processing = 1,

        /// <summary>
        /// Status : completed
        /// Hoàn thành
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Status : failed
        /// Thất bại
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Status : archived
        /// Đã lưu trữ
        /// </summary>
        Archived = 4
    }
}
