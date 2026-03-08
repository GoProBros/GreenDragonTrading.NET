using System.ComponentModel.DataAnnotations;

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
