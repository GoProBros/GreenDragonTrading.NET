using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;

namespace GreenDragonTrading.Application.Common.Utils;

public static class SubscriptionAccessHelper
{
    public static async Task<Subscription?> GetEffectiveSubscriptionAsync(
        IUnitOfWork uow,
        bool isAdminOrStaff,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (isAdminOrStaff)
        {
            return await uow.Subscriptions.GetActiveAdminSubscriptionAsync(cancellationToken);
        }

        var activeSubscription = await uow.UserSubscriptions.GetActiveSubscriptionAsync(userId, cancellationToken);
        if (activeSubscription?.Subscription != null)
        {
            return activeSubscription.Subscription;
        }

        return await uow.Subscriptions.GetActiveFreeSubscriptionAsync(cancellationToken);
    }
}