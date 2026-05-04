using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IWorkspaceRepository : IPostgreSqlGenericRepository<Workspace>
    {
        /// <summary>
        /// Get workspaces by user ID
        /// </summary>
        Task<List<Workspace>> GetWorkspaceByUserIdAsync(
            Guid userId,
            WorkspaceType? type = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get workspace by share code
        /// </summary>
        Task<Workspace?> GetByShareCodeAsync(string shareCode, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Check if share code exists
        /// </summary>
        Task<bool> ShareCodeExistsAsync(string shareCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the system default workspace (UserId is null and IsDefault is true)
        /// </summary>
        Task<Workspace?> GetSystemDefaultWorkspaceAsync(
            WorkspaceType? type = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all system workspaces (UserId is null)
        /// </summary>
        Task<List<Workspace>> GetSystemWorkspacesAsync(
            WorkspaceType? type = null,
            CancellationToken cancellationToken = default);
    }
}
