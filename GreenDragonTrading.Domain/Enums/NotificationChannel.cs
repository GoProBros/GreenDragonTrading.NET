using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Domain.Enums
{
    public enum NotificationChannel : short
    {
        [Display(Name = "System")]
        System = 1,

        [Display(Name = "Message")]
        Message = 2,

        [Display(Name = "Telegram")]
        Telegram = 3,
    }
}
