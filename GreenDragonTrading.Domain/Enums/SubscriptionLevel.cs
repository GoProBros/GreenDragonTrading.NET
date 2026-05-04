using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Domain.Enums
{
    public enum SubscriptionLevel : short
    {
        [Display(Name = "Free")]
        Free = 0,

        [Display(Name = "Vip")]
        Advanced = 1,

        [Display(Name = "Vip 1")]
        VipOne = 2,

        [Display(Name = "Vip 2")]
        VipTwo = 3,

        [Display(Name = "Vip 3")]
        VipThree = 4
    }
}
