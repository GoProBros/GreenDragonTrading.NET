namespace GreenDragonTrading.Domain.Enums
{
    /// <summary>
    /// Symbol type enum
    /// Phân loại mã chứng khoán
    /// </summary>
    public enum SymbolType : short
    {
        /// <summary>
        /// Stock type : stock
        /// Cổ phiếu
        /// </summary>
        Stock = 1,

        /// <summary>
        /// Stock type : covered warrant
        /// Chứng quyền có bảo đảm
        /// </summary>
        CoveredWarrant = 2,

        /// <summary>
        /// Stock type : futures
        /// Hợp đồng tương lai
        /// </summary>
        Futures = 3,

        /// <summary>
        /// Stock type : ETF
        /// Quỹ hoán đổi danh mục
        /// </summary>
        ETF = 4,

        /// <summary>
        /// Stock type : bond
        /// Trái phiếu
        /// </summary>
        BOND = 5,

        /// <summary>
        /// Stock type : OEF
        /// Quỹ mở
        /// </summary>
        OEF = 6,

        /// <summary>
        /// Stock type : mutual fund
        /// Quỹ tương hỗ
        /// </summary>
        MutualFund = 7,

        /// <summary>
        /// Stock type : unknown
        /// Không xác định
        /// </summary>
        Unknown = 99
    }

}
