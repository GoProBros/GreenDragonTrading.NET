using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Domain.Enums
{
    public enum TransactionType
    {
        [Display(Name = "Purchase")]
        Purchase = 1, 
        
        [Display(Name = "Upgrade")]
        Upgrade = 2  
    }
}
