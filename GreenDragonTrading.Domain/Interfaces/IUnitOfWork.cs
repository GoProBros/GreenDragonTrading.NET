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
        IAnalysisReportRepository AnalysisReports { get; }
        IAnalysisReportSourceRepository AnalysisReportSources { get; }
        IAnalysisReportCategoryRepository AnalysisReportCategories { get; }
        IMarketIndexRepository MarketIndices { get; }
        IMarketIndexSymbolRepository MarketIndexSymbols { get; }
        IChatSessionRepository ChatSessions { get; }
        IChatMessageRepository ChatMessages { get; }
        IChatParticipantRepository ChatParticipants { get; }
        IAlertRepository Alerts { get; }
        INewsArticleRepository NewsArticles { get; }
        IMacroeconomicDataRepository MacroeconomicData { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
