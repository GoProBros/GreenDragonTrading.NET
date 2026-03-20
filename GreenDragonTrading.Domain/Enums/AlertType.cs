using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Domain.Enums
{
    public enum AlertType : short
    {
        [Display(Name = "Price Alert")]
        Price = 1,

        [Display(Name = "Volume Alert")]
        Volume = 2,
    }
}
