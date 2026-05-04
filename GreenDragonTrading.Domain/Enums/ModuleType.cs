using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Domain.Enums
{
    public enum ModuleType : short
    {
        [Display(Name = "Stock Screen")]
        StockScreener = 1,

        [Display(Name = "Stock Chart")]
        Chart = 2,

        [Display(Name = "BCTC Pro")]
        BctcPro = 3,

        [Display(Name = "Heatmap")]
        Heatmap = 4

    }
}
