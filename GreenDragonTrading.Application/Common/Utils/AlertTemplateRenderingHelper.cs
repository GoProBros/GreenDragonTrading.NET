using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using System.Text.RegularExpressions;

namespace GreenDragonTrading.Application.Common.Utils;

public static class AlertTemplateRenderingHelper
{
    private static readonly Regex PlaceholderRegex = new(
        "{{\\s*(?<key>[^}\\s]+)\\s*}}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static IReadOnlyDictionary<string, string> BuildContext(
        Alert alert,
        decimal currentPrice,
        decimal currentVolume,
        decimal pricePercentUp,
        decimal pricePercentDown,
        decimal volumePercentUp,
        decimal volumePercentDown)
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Ticker"] = alert.Ticker,
            ["Mã Chứng Khoán"] = alert.Ticker,
            ["ThresholdValue"] = alert.ThresholdValue?.ToString("0.####") ?? string.Empty,
            ["Giá Định Mức"] = alert.ThresholdValue?.ToString("0.####") ?? string.Empty,
            ["ChangePercentage"] = alert.ChangePercentage?.ToString("0.##") ?? string.Empty,
            ["Phần Trăm Thay Đổi"] = alert.ChangePercentage?.ToString("0.##") ?? string.Empty,
            ["Type"] = alert.Type.GetDisplayName(),
            ["Loại Cảnh Báo"] = alert.Type.GetDisplayName(),
            ["Condition"] = alert.Condition.GetDisplayName(),
            ["Điều Kiện"] = alert.Condition.GetDisplayName(),
            ["CurrentPrice"] = currentPrice.ToString("0.####"),
            ["Giá Hiện Tại"] = currentPrice.ToString("0.####"),
            ["CurrentVolume"] = currentVolume.ToString("0.####"),
            ["Khối Lượng Hiện Tại"] = currentVolume.ToString("0.####"),
            ["PricePercentUp"] = pricePercentUp.ToString("0.##"),
            ["Tăng Giá %"] = pricePercentUp.ToString("0.##"),
            ["PricePercentDown"] = pricePercentDown.ToString("0.##"),
            ["Giảm Giá %"] = pricePercentDown.ToString("0.##"),
            ["VolumePercentUp"] = volumePercentUp.ToString("0.##"),
            ["Tăng Khối Lượng %"] = volumePercentUp.ToString("0.##"),
            ["VolumePercentDown"] = volumePercentDown.ToString("0.##"),
            ["Giảm Khối Lượng %"] = volumePercentDown.ToString("0.##"),
            ["VolumeTimeFrame"] = alert.VolumeTimeFrame?.ToString() ?? string.Empty,
            ["Khung Thời Gian"] = alert.VolumeTimeFrame?.ToString() ?? string.Empty,
            ["VolumeLookbackBars"] = alert.VolumeLookbackBars?.ToString() ?? string.Empty,
            ["Số Nến So Sánh"] = alert.VolumeLookbackBars?.ToString() ?? string.Empty,
            ["Time"] = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).ToString("HH:mm:ss dd/MM/yyyy"),
            ["Thời Gian"] = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).ToString("HH:mm:ss dd/MM/yyyy")
        };
    }

    public static string RenderTemplate(string template, IReadOnlyDictionary<string, string> context)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }

        return PlaceholderRegex.Replace(template, match =>
        {
            var key = match.Groups["key"].Value;
            return context.TryGetValue(key, out var value) ? value : match.Value;
        });
    }

    public static HashSet<string> ExtractPlaceholders(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        return PlaceholderRegex
            .Matches(template)
            .Select(match => match.Groups["key"].Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public static string BuildAlertMessage(
        Alert alert,
        decimal currentPrice,
        decimal currentVolume,
        decimal pricePercentUp,
        decimal pricePercentDown,
        decimal volumePercentUp,
        decimal volumePercentDown,
        AlertTemplate? template)
    {
        var context = BuildContext(
            alert,
            currentPrice,
            currentVolume,
            pricePercentUp,
            pricePercentDown,
            volumePercentUp,
            volumePercentDown);
        if (template != null && !string.IsNullOrWhiteSpace(template.BodyTemplate))
        {
            return RenderTemplate(template.BodyTemplate, context);
        }

        return "Lỗi lấy chuỗi";
    }
}
