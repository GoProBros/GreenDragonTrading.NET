using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Application.Interfaces;

/// <summary>
/// Service for duplicating workspaces and their associated module layouts
/// </summary>
public interface IWorkspaceDuplicationService
{
    /// <summary>
    /// Duplicates a workspace for a target user, including all referenced module layouts
    /// </summary>
    /// <param name="sourceWorkspace">The workspace to duplicate</param>
    /// <param name="targetUserId">The user ID who will own the new workspace</param>
    /// <param name="workspaceNameSuffix">Suffix to append to workspace name (e.g., "(Sao chép)")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created workspace</returns>
    Task<Workspace> DuplicateWorkspaceAsync(
        Workspace sourceWorkspace,
        Guid targetUserId,
        string workspaceNameSuffix = "(Sao chép)",
        CancellationToken cancellationToken = default);
}
