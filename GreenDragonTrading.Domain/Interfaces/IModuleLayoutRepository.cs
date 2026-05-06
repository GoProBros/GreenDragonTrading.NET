using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Domain.Interfaces;

/// <summary>
/// Repository interface cho ModuleLayout
/// </summary>
public interface IModuleLayoutRepository : IPostgreSqlGenericRepository<ModuleLayout>
{
    /// <summary>
    /// Lấy danh sách layout theo loại module.
    /// Normal user chỉ thấy layout của chính mình. Admin/Staff thấy của mình + system default.
    /// </summary>
    /// <param name="moduleType">Loại module</param>
    /// <param name="userId">ID người dùng</param>
    /// <param name="includeSystemDefaults">Nếu true, bao gồm cả system default layout (dành cho admin/staff)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Danh sách layout</returns>
    Task<List<ModuleLayout>> GetByModuleTypeAsync(
        ModuleType moduleType, 
        Guid userId,
        bool includeSystemDefaults = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy layout theo ID
    /// </summary>
    /// <param name="id">ID layout</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Layout hoặc null</returns>
    Task<ModuleLayout?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy layout theo ID, giới hạn quyền truy cập.
    /// Normal user chỉ truy cập layout của mình. Admin/Staff truy cập được của mình + system default.
    /// </summary>
    /// <param name="id">ID layout</param>
    /// <param name="userId">ID người dùng</param>
    /// <param name="includeSystemDefaults">Nếu true, bao gồm cả system default layout</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Layout hoặc null nếu không có quyền</returns>
    Task<ModuleLayout?> GetByIdAndUserIdAsync(
        long id, 
        Guid userId,
        bool includeSystemDefaults = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra layout có tồn tại và thuộc về user không
    /// </summary>
    /// <param name="id">ID layout</param>
    /// <param name="userId">ID người dùng</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True nếu layout thuộc về user</returns>
    Task<bool> IsOwnedByUserAsync(
        long id, 
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa layout
    /// </summary>
    /// <param name="layout">Layout cần xóa</param>
    void Delete(ModuleLayout layout);
}
