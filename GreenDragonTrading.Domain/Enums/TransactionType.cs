using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
