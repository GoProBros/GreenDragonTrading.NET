using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenDragonTrading.Domain.Enums
{
    public enum PaymentType : short
    {
        [Display(Name = "Payos")]
        Payos = 1,

        [Display(Name = "Momo")]
        Momo = 2,

        [Display(Name = "VnPay")]
        VnPay = 3
    }
}
