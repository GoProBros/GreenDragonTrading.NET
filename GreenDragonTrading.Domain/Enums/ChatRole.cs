using System.ComponentModel.DataAnnotations;

namespace GreenDragonTrading.Domain.Enums
{
    public enum ChatRole : short
    {
        [Display(Name = "Admin")]
        Admin = 1,

        [Display(Name = "Member")]
        Member = 2
    }
}
