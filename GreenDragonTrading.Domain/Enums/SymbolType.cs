namespace GreenDragonTrading.Domain.Enums
{
    /// <summary>
    /// Phân loại mã chứng khoán
    /// </summary>
    public enum SymbolType : short
    {
        /// <summary>
        /// Cổ phiếu
        /// </summary>
        Stock = 1,

        /// <summary>
        /// Quỹ hoán đổi danh mục
        /// </summary>
        ETF = 2,

        /// <summary>
        /// Trái phiếu
        /// </summary>
        Bond = 3,

        /// <summary>
        /// Chứng quyền có bảo đảm
        /// </summary>
        CoveredWarrant = 4,

        /// <summary>
        /// Chứng chỉ quỹ
        /// </summary>
        FundCertificate = 5
    }
}
