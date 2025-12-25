using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GreenDragonTrading.Domain.Enums
{
    public enum SubscriptionLevel : short
    {
        /// <summary>
        /// Free subscription level with basic features for every user.
        /// </summary>
        [Display(Name = "Free")]
        Free = 0,

        /// <summary>
        /// Specifies that the feature or option is intended for advanced users or scenarios.
        /// </summary>
        [Display(Name = "Advanced")]
        Advanced = 1,

        /// <summary>
        /// Specifies a premium subscription level.
        /// </summary>
        [Display(Name = "Premium")]
        Premium = 2,
    }
}
