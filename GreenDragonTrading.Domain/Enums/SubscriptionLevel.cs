using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Domain.Enums
{
    public enum SubscriptionLevel : short
    {
        [Display(Name = "Free")]
        Free = 0,

        [Display(Name = "Vip")]
        Advanced = 1
    }
}
