using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace GreenDragonTrading.Infrastructure.Services;

public class NewsRssService : INewsRssService
{
    private static readonly Regex ImageSrcRegex = new("<img[^>]*src=[\"'](?<src>[^\"']+)[\"'][^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex HtmlTagRegex = new("<.*?>", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new("\\s+", RegexOptions.Compiled);
    private static readonly Regex NoiseLineRegex = new("(copy\\s*link|chia\\s*se|chia\\s*sẻ|tu\\s*khoa|từ\\s*khóa|cung\\s*chuyen\\s*muc|cùng\\s*chuyên\\s*mục|moi\\s*nhat|mới\\s*nhất|tin\\s*moi|tin\\s*mới|hotline|lien\\s*he\\s*quang\\s*cao|liên\\s*hệ\\s*quảng\\s*cáo|theo\\s+nhịp\\s+sống\\s+thị\\s+trường)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly string[] RssDateFormats =
    [
        "ddd, dd MMM yy HH:mm:ss zzz",
        "ddd, dd MMM yyyy HH:mm:ss zzz",
        "r"
    ];

    private readonly HttpClient _httpClient;
    private readonly NewsRssOptions _options;
    private readonly ILogger<NewsRssService> _logger;

    public NewsRssService(
        HttpClient httpClient,
        IOptions<NewsRssOptions> options,
        ILogger<NewsRssService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<List<RssNewsItemDto>> FetchCafeFStockNewsAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.CafeFStockRssUrl))
        {
            throw new InvalidOperationException("NewsRss:CafeFStockRssUrl is not configured.");
        }

        _logger.LogInformation("Fetching CafeF RSS from {RssUrl}", _options.CafeFStockRssUrl);

        var rssContent = await _httpClient.GetStringAsync(_options.CafeFStockRssUrl, cancellationToken);
        var document = XDocument.Parse(rssContent);

        var itemElements = document.Descendants("item").ToList();
        var results = new List<RssNewsItemDto>(itemElements.Count);

        foreach (var item in itemElements)
        {
            var link = item.Element("link")?.Value?.Trim() ?? string.Empty;
            var title = WebUtility.HtmlDecode(item.Element("title")?.Value?.Trim() ?? string.Empty);
            var descriptionRaw = item.Element("description")?.Value?.Trim() ?? string.Empty;
            var pubDateRaw = item.Element("pubDate")?.Value?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(link) || string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            var thumbnailUrl = ExtractThumbnailUrl(descriptionRaw);
            var content = await FetchArticleContentAsync(link, descriptionRaw, cancellationToken);

            if (!TryParsePublishedAt(pubDateRaw, out var publishedAt))
            {
                publishedAt = DateTimeOffset.UtcNow;
            }

            results.Add(new RssNewsItemDto
            {
                Title = title,
                Content = content,
                Link = link,
                ThumbnailUrl = thumbnailUrl,
                PublishedAt = publishedAt,
                Summary = null
            });
        }

        _logger.LogInformation("Fetched {Count} news items from CafeF RSS", results.Count);

        return results;
    }

    private static string? ExtractThumbnailUrl(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var match = ImageSrcRegex.Match(html);
        return match.Success ? match.Groups["src"].Value.Trim() : null;
    }

    private async Task<string> FetchArticleContentAsync(string link, string descriptionRaw, CancellationToken cancellationToken)
    {
        try
        {
            var articleHtml = await _httpClient.GetStringAsync(link, cancellationToken);
            var articleContentHtml = ExtractArticleContentHtml(articleHtml);

            if (string.IsNullOrWhiteSpace(articleContentHtml))
            {
                _logger.LogWarning("Could not extract article body from link {Link}. Fallback to RSS description.", link);
                return NormalizeText(descriptionRaw);
            }

            return NormalizeText(articleContentHtml);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch article content from link {Link}. Fallback to RSS description.", link);
            return NormalizeText(descriptionRaw);
        }
    }

    private static string ExtractArticleContentHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var contentNode = doc.DocumentNode.SelectSingleNode("//div[@data-role='content']");
        if (contentNode == null)
        {
            var fallbackNode = doc.DocumentNode.SelectSingleNode("//article")
                ?? doc.DocumentNode.SelectSingleNode("//div[contains(@class,'detail-content')]");

            if (fallbackNode == null)
            {
                return string.Empty;
            }

            contentNode = fallbackNode;
        }

        var junkNodes = contentNode.SelectNodes(
            ".//script"
            + " | .//style"
            + " | .//div[contains(@class,'c-banner')]"
            + " | .//div[contains(@class,'h-show-pc')]"
            + " | .//div[contains(@class,'h-show-mobile')]"
            + " | .//div[contains(@class,'tindnd')]"
            + " | .//span[contains(@class,'title_box')]"
            + " | .//ul[@id='aiservice-lastest-news']");

        if (junkNodes != null)
        {
            foreach (var node in junkNodes)
            {
                node.Remove();
            }
        }

        return contentNode.InnerHtml;
    }

    private static string NormalizeText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml($"<div>{html}</div>");

        var paragraphNodes = doc.DocumentNode.SelectNodes("//p | //blockquote");
        if (paragraphNodes == null || paragraphNodes.Count == 0)
        {
            var plain = HtmlTagRegex.Replace(html, " ");
            plain = WebUtility.HtmlDecode(plain);
            plain = WhitespaceRegex.Replace(plain, " ").Trim();
            return NoiseLineRegex.IsMatch(plain) ? string.Empty : plain;
        }

        var lines = paragraphNodes
            .Select(x => WebUtility.HtmlDecode(x.InnerText))
            .Select(x => WhitespaceRegex.Replace(x, " ").Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Where(x => !NoiseLineRegex.IsMatch(x))
            .ToList();

        return string.Join("\n\n", lines);
    }

    private static bool TryParsePublishedAt(string pubDateRaw, out DateTimeOffset publishedAt)
    {
        return DateTimeOffset.TryParseExact(
                   pubDateRaw,
                   RssDateFormats,
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.AllowWhiteSpaces,
                   out publishedAt)
               || DateTimeOffset.TryParse(
                   pubDateRaw,
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.AllowWhiteSpaces,
                   out publishedAt);
    }
}
