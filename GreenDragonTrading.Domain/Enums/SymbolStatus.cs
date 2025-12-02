namespace GreenDragonTrading.Domain.Enums
{
    /// <summary>
    /// Trạng thái của chứng khoán
    /// Symbol status
    /// </summary>
    public enum SymbolStatus : short
    {
        /// <summary>
        /// Giao dịch bình thường
        /// Symbol status : Normal trading
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Bị huỷ niêm yết
        /// Symbol status : Delisted
        /// </summary>
        Delisted = 2,

        /// <summary>
        /// Tạm dừng giao dịch giữa phiên
        /// Symbol status : Halted
        /// </summary>
        Halt = 3,

        /// <summary>
        /// Ngừng giao dịch
        /// Symbol status : Suspended
        /// </summary>
        Suspend = 4,

        /// <summary>
        /// Niêm yết mới
        /// Symbol status : New listing
        /// </summary>
        NewList = 5,

        /// <summary>
        /// Sắp huỷ niêm yết
        /// Symbol status : Near delisted
        /// </summary>
        NearDelist = 6,

        /// <summary>
        /// Giao dịch đặc biệt
        /// Symbol status : Special trading
        /// </summary>
        SpecialTrading = 7,
        
        /// <summary>
        /// Bị ngưng giao dịch khớp lệnh
        /// Symbol status : Suspended A
        /// </summary>
        SuspendA = 8,

        /// <summary>
        /// Bị ngưng giao dịch khớp lệnh thoả thuận
        /// Symbol status : Suspended PT
        /// </summary>
        SuspendPT = 9,

        /// <summary>
        /// Không xác định
        /// Symbol status : Unknown
        /// </summary>
        Unknown = 99
    }
}
