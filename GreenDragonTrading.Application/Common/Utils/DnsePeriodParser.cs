namespace GreenDragonTrading.Application.Common.Utils;

/// <summary>
/// Utility class for parsing and formatting DNSE period strings
/// </summary>
public static class DnsePeriodParser
{
    /// <summary>
    /// Parse period string from DNSE format (e.g., "Q3/2024", "2024") to Year and Quarter
    /// </summary>
    /// <param name="periodString">Period string in DNSE format</param>
    /// <returns>Tuple of (year, quarter) where quarter is null for yearly periods</returns>
    /// <exception cref="ArgumentException">When period string is null or empty</exception>
    /// <exception cref="FormatException">When period string format is invalid</exception>
    public static (int year, int? quarter) ParsePeriodString(string periodString)
    {
        if (string.IsNullOrWhiteSpace(periodString))
            throw new ArgumentException("Period string cannot be null or empty", nameof(periodString));
        
        // Handle yearly format: "2024"
        if (int.TryParse(periodString, out var year))
        {
            return (year, null);
        }
        
        // Handle quarterly format: "Q3/2024"
        var parts = periodString.Split('/');
        if (parts.Length == 2 && parts[0].StartsWith('Q'))
        {
            var quarterStr = parts[0][1..]; // Remove 'Q' prefix
            if (int.TryParse(quarterStr, out var quarter) && int.TryParse(parts[1], out var yearFromQuarter))
            {
                return (yearFromQuarter, quarter);
            }
        }
        
        throw new FormatException($"Invalid period format: {periodString}. Expected formats: 'Q3/2024' or '2024'");
    }
    
    /// <summary>
    /// Format period to DNSE format
    /// </summary>
    /// <param name="year">Year value</param>
    /// <param name="quarter">Optional quarter value (1-4)</param>
    /// <returns>Formatted period string (e.g., "Q3/2024" or "2024")</returns>
    public static string FormatPeriod(int year, int? quarter = null)
    {
        return quarter.HasValue ? $"Q{quarter}/{year}" : year.ToString();
    }
}
