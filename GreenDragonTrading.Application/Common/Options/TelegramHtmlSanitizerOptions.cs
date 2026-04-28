namespace GreenDragonTrading.Application.Common.Options;

public sealed class TelegramHtmlSanitizerOptions
{
    public static TelegramHtmlSanitizerOptions Default { get; } = new();

    public IReadOnlyCollection<string> AllowedTags { get; init; } =
    [
        "b",
        "i",
        "u",
        "s",
        "a",
        "code",
        "pre",
        "tg-spoiler",
        "blockquote"
    ];

    public IReadOnlyCollection<string> AllowedSchemes { get; init; } =
    [
        "http",
        "https",
        "mailto",
        "tg"
    ];

    public string ListItemPrefix { get; init; } = "• ";

    public string ParagraphSeparator { get; init; } = "\n\n";

    public string BlockSeparator { get; init; } = "\n";

    public bool ConvertHeadingsToBold { get; init; } = true;

    public int MaxConsecutiveNewlines { get; init; } = 2;
}
