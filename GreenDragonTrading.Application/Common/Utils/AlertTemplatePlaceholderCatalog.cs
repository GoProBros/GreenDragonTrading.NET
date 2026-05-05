namespace GreenDragonTrading.Application.Common.Utils;

public static class AlertTemplatePlaceholderCatalog
{
    public static IReadOnlyList<AlertTemplatePlaceholderDefinition> GetDefinitions()
    {
        return
        [
            new AlertTemplatePlaceholderDefinition(
                "Ticker",
                "Mã Chứng Khoán",
                "Alert",
                "Mã chứng khoán của cảnh báo."),
            new AlertTemplatePlaceholderDefinition(
                "ThresholdValue",
                "Giá Định Mức",
                "Alert",
                "Giá định mức của cảnh báo."),
            new AlertTemplatePlaceholderDefinition(
                "ChangePercentage",
                "Phần Trăm Thay Đổi",
                "Alert",
                "Phần trăm thay đổi đã cấu hình, nếu có."),
            new AlertTemplatePlaceholderDefinition(
                "Type",
                "Loại Cảnh Báo",
                "Display",
                "Tên hiển thị của loại cảnh báo (GetDisplayName)."),
            new AlertTemplatePlaceholderDefinition(
                "Condition",
                "Điều Kiện",
                "Display",
                "Tên hiển thị của điều kiện cảnh báo (GetDisplayName)."),
            new AlertTemplatePlaceholderDefinition(
                "CurrentPrice",
                "Giá Hiện Tại",
                "Realtime",
                "Giá hiện tại tại thời điểm kích hoạt."),
            new AlertTemplatePlaceholderDefinition(
                "CurrentVolume",
                "Khối Lượng Hiện Tại",
                "Realtime",
                "Khối lượng hiện tại tại thời điểm kích hoạt."),
            new AlertTemplatePlaceholderDefinition(
                "Time",
                "Thời Gian",
                "Realtime",
                "Thời gian kích hoạt theo múi giờ Việt Nam.")
        ];
    }

    public static string ToToken(string tokenKey)
    {
        return $"{{{{{tokenKey}}}}}";
    }
}

public sealed record AlertTemplatePlaceholderDefinition(
    string TokenKey,
    string Key,
    string Category,
    string Description);
