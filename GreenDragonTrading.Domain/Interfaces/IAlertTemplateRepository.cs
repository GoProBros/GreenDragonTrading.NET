using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Domain.Interfaces
{
    public interface IAlertTemplateRepository : IPostgreSqlGenericRepository<AlertTemplate>
    {
        Task<AlertTemplate?> GetActiveByTypeAndConditionAsync(
            AlertType type,
            ConditionType condition,
            CancellationToken cancellationToken = default);

        Task<AlertTemplate?> GetDefaultAsync(CancellationToken cancellationToken = default);
    }
}
