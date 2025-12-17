namespace GreenDragonTrading.Domain.Enums
{
    public enum  SubscriptionStatus
    {
        /// <summary>
        /// Subscription is currently active
        /// </summary>
        Active = 1,

        /// <summary>
        /// Subscription has expired
        /// </summary>
        Expired = 2,

        /// <summary>
        /// Subscription has been canceled
        /// </summary>
        Canceled = 3,

        /// <summary>
        /// Indicates that the connection has been upgraded to a higher protocol version or capability.
        /// </summary>
        Upgraded = 4
    }
}
