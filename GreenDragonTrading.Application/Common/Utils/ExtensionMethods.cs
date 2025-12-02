namespace GreenDragonTrading.Application.Common.Utils
{
    public static class ExtensionMethods
    {
        public static string ToQueryString(this object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             let value = p.GetValue(obj, null)
                             where value != null
                             select p.Name + "=" + Uri.EscapeDataString(
                                 value is DateTime date
                                 ? date.ToString("yyyy-MM-ddTHH:mm:ss")
                                 : value.ToString()!
                             );
            return $"?{String.Join("&", properties.ToArray())}";
        }
    }
}
