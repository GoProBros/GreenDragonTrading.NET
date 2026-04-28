using System.Net;
using System.Text.RegularExpressions;
using System.Linq;
using GreenDragonTrading.Application.Common.Options;

namespace GreenDragonTrading.Application.Common.Utils;

public static class TelegramHtmlSanitizer
{
    private static readonly Regex ScriptStyleRegex = new(
        "<\\s*(script|style)\\b[^>]*>.*?<\\s*/\\s*\\1\\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex HeadingRegex = new(
        "<h[1-6][^>]*>(?<content>.*?)</h[1-6]>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex ParagraphOpenRegex = new(
        "<p\\b[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex ParagraphCloseRegex = new(
        "</p>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex BreakRegex = new(
        "<br\\s*/?>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex ListOpenRegex = new(
        "<(ul|ol)\\b[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex ListCloseRegex = new(
        "</(ul|ol)>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex ListItemOpenRegex = new(
        "<li\\b[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex ListItemCloseRegex = new(
        "</li>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex AnchorRegex = new(
        "<a\\b[^>]*>(?<inner>.*?)</a>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex AnchorHrefRegex = new(
        "href\\s*=\\s*(?:\"(?<value>[^\"]*)\"|'(?<value>[^']*)'|(?<value>[^\\s>]+))",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex TagRegex = new(
        "</?(?<tag>[a-z0-9-]+)\\b[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static string PrepareHtml(string feHtml, TelegramHtmlSanitizerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(feHtml))
        {
            return string.Empty;
        }

        options ??= TelegramHtmlSanitizerOptions.Default;
        var processed = NormalizeLineBreaks(feHtml);

        processed = ScriptStyleRegex.Replace(processed, string.Empty);

        if (options.ConvertHeadingsToBold)
        {
            processed = HeadingRegex.Replace(processed, match =>
            {
                var content = match.Groups["content"].Value;
                return $"{options.BlockSeparator}<b>{content}</b>{options.BlockSeparator}";
            });
        }

        processed = ListOpenRegex.Replace(processed, options.BlockSeparator);
        processed = ListCloseRegex.Replace(processed, options.BlockSeparator);
        processed = ListItemOpenRegex.Replace(processed, options.ListItemPrefix);
        processed = ListItemCloseRegex.Replace(processed, "\n");

        processed = ParagraphOpenRegex.Replace(processed, string.Empty);
        processed = ParagraphCloseRegex.Replace(processed, options.ParagraphSeparator);
        processed = BreakRegex.Replace(processed, "\n");

        processed = NormalizeInlineTag(processed, "strong", "b");
        processed = NormalizeInlineTag(processed, "em", "i");
        processed = NormalizeInlineTag(processed, "strike", "s");
        processed = NormalizeInlineTag(processed, "del", "s");

        processed = AnchorRegex.Replace(processed, match => SanitizeAnchor(match, options));

        processed = NormalizeAllowedTags(processed);
        processed = RemoveDisallowedTags(processed, options);
        processed = EscapeTextPreservingAllowedTags(processed, options);
        processed = CollapseNewlines(processed, options.MaxConsecutiveNewlines);

        return processed.Trim();
    }

    private static string NormalizeLineBreaks(string input)
    {
        return input.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);
    }

    private static string NormalizeInlineTag(string input, string sourceTag, string targetTag)
    {
        var openPattern = $"<\\s*{Regex.Escape(sourceTag)}\\b[^>]*>";
        var closePattern = $"<\\s*/\\s*{Regex.Escape(sourceTag)}\\s*>";
        input = Regex.Replace(input, openPattern, $"<{targetTag}>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        input = Regex.Replace(input, closePattern, $"</{targetTag}>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return input;
    }

    private static string SanitizeAnchor(Match match, TelegramHtmlSanitizerOptions options)
    {
        var inner = match.Groups["inner"].Value;
        var hrefMatch = AnchorHrefRegex.Match(match.Value);
        if (!hrefMatch.Success)
        {
            return inner;
        }

        var href = hrefMatch.Groups["value"].Value.Trim();
        if (!IsAllowedHref(href, options))
        {
            return inner;
        }

        var encodedHref = WebUtility.HtmlEncode(href);
        return $"<a href=\"{encodedHref}\">{inner}</a>";
    }

    private static bool IsAllowedHref(string href, TelegramHtmlSanitizerOptions options)
    {
        if (!Uri.TryCreate(href, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return options.AllowedSchemes.Any(scheme =>
            scheme.Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeAllowedTags(string input)
    {
        return Regex.Replace(
            input,
            "<(\\/?)\\s*(b|i|u|s|code|pre|tg-spoiler|blockquote)\\b[^>]*>",
            match => $"<{match.Groups[1].Value}{match.Groups[2].Value}>",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static string RemoveDisallowedTags(string input, TelegramHtmlSanitizerOptions options)
    {
        var allowedTags = new HashSet<string>(options.AllowedTags, StringComparer.OrdinalIgnoreCase);
        return TagRegex.Replace(input, match =>
        {
            var tag = match.Groups["tag"].Value;
            return allowedTags.Contains(tag) ? match.Value : string.Empty;
        });
    }

    private static string EscapeTextPreservingAllowedTags(string input, TelegramHtmlSanitizerOptions options)
    {
        var tagPattern = string.Join("|", options.AllowedTags.Select(Regex.Escape));
        var allowedTagRegex = new Regex(
            $"</?(?:{tagPattern})\\b[^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        var tokens = new List<string>();
        var placeholderText = allowedTagRegex.Replace(input, match =>
        {
            var index = tokens.Count;
            tokens.Add(match.Value);
            return $"\u0001{index}\u0001";
        });

        placeholderText = WebUtility.HtmlEncode(placeholderText);

        for (var i = 0; i < tokens.Count; i++)
        {
            placeholderText = placeholderText.Replace($"\u0001{i}\u0001", tokens[i], StringComparison.Ordinal);
        }

        return placeholderText;
    }

    private static string CollapseNewlines(string input, int maxNewlines)
    {
        if (maxNewlines < 1)
        {
            return input;
        }

        var pattern = $"\\n{{{maxNewlines + 1},}}";
        var replacement = new string('\n', maxNewlines);
        return Regex.Replace(input, pattern, replacement, RegexOptions.CultureInvariant);
    }
}
