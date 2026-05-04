using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Domain.Enums
{
    public enum ChatSessionType : short
    {
        [Display (Name = "System")]
        System = 0,

        [Display(Name = "AI")]
        AI = 1,

        [Display(Name = "Direct")]
        Direct = 2,

        [Display(Name = "Group")]
        Group = 3
    }
}
