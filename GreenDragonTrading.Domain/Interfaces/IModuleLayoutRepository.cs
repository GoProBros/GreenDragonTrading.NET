using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Domain.Interfaces;

/// <summary>
/// Repository interface cho ModuleLayout
/// </summary>
public interface IModuleLayoutRepository : IPostgreSqlGenericRepository<ModuleLayout>
{
    /// <summary>
    /// Lấy danh sách layout theo loại module
    /// </summary>
    /// <param name="moduleType">Loại module</param>
    /// <param name="userId">ID người dùng (để lấy layout cá nhân)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Danh sách layout (system + personal của user)</returns>
    Task<List<ModuleLayout>> GetByModuleTypeAsync(
        ModuleType moduleType, 
        Guid? userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy layout theo ID
    /// </summary>
    /// <param name="id">ID layout</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Layout hoặc null</returns>
    Task<ModuleLayout?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy layout theo ID và user ID (để đảm bảo user chỉ truy cập layout của mình hoặc system default)
    /// </summary>
    /// <param name="id">ID layout</param>
    /// <param name="userId">ID người dùng</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Layout hoặc null</returns>
    Task<ModuleLayout?> GetByIdAndUserIdAsync(
        long id, 
        Guid userId, 
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
