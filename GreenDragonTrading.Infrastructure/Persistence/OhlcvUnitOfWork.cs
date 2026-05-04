using GreenDragonTrading.Domain.Interfaces;

namespace GreenDragonTrading.Infrastructure.Persistence
{
    /// <summary>
    /// Unit of Work pattern cho OHLCV database
    /// Quản lý transactions và repository lifecycle
    /// </summary>
    public class OhlcvUnitOfWork : IOhlcvUnitOfWork, IDisposable
    {
        private readonly OhlcvTimescaleDbContext _context;
        private IOhlcvRepository? _ohlcvRepository;

        public OhlcvUnitOfWork(OhlcvTimescaleDbContext context)
        {
            _context = context;
        }

        public IOhlcvRepository Ohlcv
        {
            get
            {
                _ohlcvRepository ??= new Repositories.OhlcvRepository(_context);
                return _ohlcvRepository;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}