namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ISymbolRepository Symbols { get; }
        ISectorRepository Sectors { get; }
        IUserRepository Users { get; }
        IUserSubscriptionRepository UserSubscriptions { get; }
        ISubscriptionRepository Subscriptions { get; }
        ITransactionRepository Transactions { get; }
        IWorkspaceRepository Workspaces { get; }
        IModuleLayoutRepository ModuleLayouts { get; }
        IWatchListRepository WatchLists { get; }
        IFinancialReportRepository FinancialReports { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
