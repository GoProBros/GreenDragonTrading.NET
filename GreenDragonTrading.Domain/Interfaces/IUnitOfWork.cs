namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ISymbolRepository Symbols { get; }
        ISectorRepository Sectors { get; }
        IUserRepository Users { get; }
        IUserSubscriptionRepository UserSubscriptions { get; }
        IWorkspaceRepository Workspaces { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
