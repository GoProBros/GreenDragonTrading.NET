using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Domain.Enums
{
    public enum WorkspaceType : short
    {
        [Display(Name = "Web")]
        Web = 1,

        [Display(Name = "Mobile")]
        Mobile = 2
    }
}
