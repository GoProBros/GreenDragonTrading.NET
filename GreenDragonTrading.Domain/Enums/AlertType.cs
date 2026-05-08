using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Domain.Enums
{
    public enum AlertType : short
    {
        [Display(Name = "Cảnh báo giá")]
        Price = 1,

        [Display(Name = "Cảnh báo khối lượng")]
        Volume = 2,
    }
}
