using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GreenDragonTrading.Domain.Enums
{
    public enum UserRole : short
    {
        [Display(Name = "Người dùng")]
        User = 1,

        [Display(Name = "Nhân viên")]
        Staff = 2,

        [Display(Name = "Quản trị viên")]
        Admin = 3,
    }

    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .FirstOrDefault()?
                            .GetCustomAttribute<DisplayAttribute>()
                            ?.GetName() ?? enumValue.ToString();
        }
    }
}
