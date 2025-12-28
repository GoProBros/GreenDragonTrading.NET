using GreenDragonTrading.Domain.Interfaces;
using GreenDragonTrading.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace GreenDragonTrading.Infrastructure.Persistence
{
    public class UnitOfWork(GdtPostgreSqlDbContext context) : IUnitOfWork
    {
        private readonly GdtPostgreSqlDbContext _context = context;
        private IDbContextTransaction? _transaction;

        // Repository properties
        private ISymbolRepository? symbols;

        private ISectorRepository? sectors;

        private IUserRepository? users;
        
        private IUserSubscriptionRepository? userSubscriptions;

        private IWorkspaceRepository? workspaces;

        private IFinancialReportRepository? financialReports;

        private IModuleLayoutRepository? moduleLayouts;

        private IWatchListRepository? watchLists;

        public ISymbolRepository Symbols => symbols ??= new SymbolRepository(_context);
        public ISectorRepository Sectors => sectors ??= new SectorRepository(_context);
        public IUserRepository Users => users ??= new UserRepository(_context);
        public IUserSubscriptionRepository UserSubscriptions => userSubscriptions ??= new UserSubscriptionRepository(_context);
        public IWorkspaceRepository Workspaces => workspaces ??= new WorkspaceRepository(_context);
        public IModuleLayoutRepository ModuleLayouts => moduleLayouts ??= new ModuleLayoutRepository(_context);
        public IWatchListRepository WatchLists => watchLists ??= new WatchListRepository(_context);
        public IFinancialReportRepository FinancialReports => financialReports ??= new FinancialReportRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }

}
