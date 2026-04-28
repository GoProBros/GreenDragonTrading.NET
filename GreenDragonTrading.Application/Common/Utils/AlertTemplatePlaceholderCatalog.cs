namespace GreenDragonTrading.Application.Common.Utils;

public static class AlertTemplatePlaceholderCatalog
{
    public static IReadOnlyList<AlertTemplatePlaceholderDefinition> GetDefinitions()
    {
        return
        [
            new AlertTemplatePlaceholderDefinition(
                "Ticker",
                "Alert",
                "Alert ticker code."),
            new AlertTemplatePlaceholderDefinition(
                "ThresholdValue",
                "Alert",
                "Alert threshold value."),
            new AlertTemplatePlaceholderDefinition(
                "ChangePercentage",
                "Alert",
                "Configured change percentage, if provided."),
            new AlertTemplatePlaceholderDefinition(
                "Type",
                "Display",
                "Alert type display name (GetDisplayName)."),
            new AlertTemplatePlaceholderDefinition(
                "Condition",
                "Display",
                "Alert condition display name (GetDisplayName)."),
            new AlertTemplatePlaceholderDefinition(
                "CurrentPrice",
                "Realtime",
                "Current price at trigger time."),
            new AlertTemplatePlaceholderDefinition(
                "CurrentVolume",
                "Realtime",
                "Current volume at trigger time.")
        ];
    }

    public static string ToToken(string key)
    {
        return $"{{{{{key}}}}}";
    }
}

public sealed record AlertTemplatePlaceholderDefinition(
    string Key,
    string Category,
    string Description);
