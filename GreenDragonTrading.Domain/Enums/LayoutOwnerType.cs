namespace GreenDragonTrading.Domain.Enums;

/// <summary>
/// Loại chủ sở hữu của layout
/// </summary>
public enum LayoutOwnerType
{
    /// <summary>
    /// Layout hệ thống - mặc định, dùng chung
    /// </summary>
    System = 1,

    /// <summary>
    /// Layout cá nhân - do người dùng tạo
    /// </summary>
    Personal = 2
}
