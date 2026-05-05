using System.Globalization;
using System.Text.RegularExpressions;

namespace GreenDragonTrading.Application.Common.Utils;

public static class AlertAiNumberNormalizer
{
    private static readonly Regex NumberRegex = new(
        @"(?<![A-Za-z])(?<num>[+-]?\d[\d.,]*)(?<suffix>%?)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string NormalizeMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        return NumberRegex.Replace(message, match =>
        {
            var token = match.Groups["num"].Value;
            var suffix = match.Groups["suffix"].Value;

            if (!TryParseFlexibleNumber(token, out var value))
            {
                return match.Value;
            }

            if (!string.Equals(suffix, "%", StringComparison.Ordinal))
            {
                value = ApplyUnderFourDigitRule(value);
            }

            var formatted = value.ToString("0.####", CultureInfo.InvariantCulture);
            return string.Concat(formatted, suffix);
        });
    }

    private static decimal ApplyUnderFourDigitRule(decimal value)
    {
        var abs = Math.Abs(value);
        var integerPart = decimal.Truncate(abs);
        var integerDigits = integerPart.ToString("0", CultureInfo.InvariantCulture).Length;
        return integerDigits < 4 ? value * 1000m : value;
    }

    private static bool TryParseFlexibleNumber(string token, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var cleaned = token.Trim();
        var dotCount = CountChar(cleaned, '.');
        var commaCount = CountChar(cleaned, ',');

        if (dotCount > 0 && commaCount > 0)
        {
            var lastDot = cleaned.LastIndexOf('.');
            var lastComma = cleaned.LastIndexOf(',');
            var decimalSep = lastDot > lastComma ? '.' : ',';
            var thousandSep = decimalSep == '.' ? ',' : '.';
            cleaned = cleaned.Replace(thousandSep.ToString(), string.Empty);
            cleaned = cleaned.Replace(decimalSep, '.');
        }
        else if (dotCount > 1 || commaCount > 1)
        {
            var sep = dotCount > 1 ? '.' : ',';
            cleaned = cleaned.Replace(sep.ToString(), string.Empty);
        }
        else if (dotCount == 1 || commaCount == 1)
        {
            var sep = dotCount == 1 ? '.' : ',';
            cleaned = cleaned.Replace(sep, '.');
        }

        return decimal.TryParse(
            cleaned,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out value);
    }

    private static int CountChar(string value, char target)
    {
        var count = 0;
        foreach (var ch in value)
        {
            if (ch == target)
            {
                count++;
            }
        }

        return count;
    }
}
