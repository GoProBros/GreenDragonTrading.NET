using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Domain.Enums
{
    public enum TransactionStatus : short
    {
        [Display(Name = "Pending")]
        Pending = 0,

        [Display(Name = "Completed")]
        Completed = 1,

        [Display(Name = "Cancelled")]
        Cancelled = 2
    }
}
