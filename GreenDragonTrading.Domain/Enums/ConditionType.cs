using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Domain.Enums
{
    public enum ConditionType : short
    {
        [Display(Name = "Trên ngưỡng")]
        Above = 1,

        [Display(Name = "Dưới ngưỡng")]
        Below = 2,

        [Display(Name = "Tăng % so với giá đặt")]
        PercentChangeUp = 3,

        [Display(Name = "Giảm % so với giá đặt")]
        PercentChangeDown = 4,
    }
}
