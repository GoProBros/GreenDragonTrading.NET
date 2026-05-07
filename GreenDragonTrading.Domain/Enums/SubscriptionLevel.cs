using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Domain.Enums
{
    public enum SubscriptionLevel : short
    {
        [Display(Name = "Free")]
        Free = 0,

        [Display(Name = "Vip 1")]
        VipOne = 1,

        [Display(Name = "Vip 2")]
        VipTwo = 2,

        [Display(Name = "Vip 3")]
        VipThree = 3,

        [Display(Name = "Vip 4")]
        VipFour = 4,

        [Display(Name = "Full Access")]
        FullAccess = 99
    }
}
