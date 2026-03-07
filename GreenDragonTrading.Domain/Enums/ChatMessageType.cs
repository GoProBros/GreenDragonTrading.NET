using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenDragonTrading.Domain.Enums
{
    public enum ChatMessageType : short
    {
        [Display(Name = "Text Message")]
        Text = 0,

        [Display(Name = "Alert Message")]
        Alert = 1,

        [Display(Name = "File Message")]
        File = 2,

        [Display(Name = "System Notification")]
        SystemNotification = 3
    }
}
