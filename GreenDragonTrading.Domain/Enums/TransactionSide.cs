using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Domain.Enums
{
    public enum TransactionSide : short
    {
        [Display(Name = "Buy")]
        Buy = 1,
        
        [Display(Name = "Sell")]
        Sell = 2
    }
}
