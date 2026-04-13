using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Domain.Enums
{
    public enum PaymentType : short
    {
        [Display(Name = "Payos")]
        Payos = 1,

        [Display(Name = "Momo")]
        Momo = 2,
    }
}
