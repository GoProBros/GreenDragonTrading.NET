using System.Text.Json;

namespace GreenDragonTrading.Application.Common.Utils
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Generates a query string from the properties of an object.
        /// </summary>
        public static string ToQueryString(this object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             let value = p.GetValue(obj, null)
                             where value != null
                             select p.Name + "=" + Uri.EscapeDataString(
                                 value is DateTime date
                                 ? date.ToString("yyyy-MM-ddTHH:mm:ss")
                                 : value is bool b
                                 ? b.ToString().ToLowerInvariant()  // true/false not True/False
                                 : value.ToString()!
                             );
            return $"?{String.Join("&", properties.ToArray())}";
        }
    }
}
