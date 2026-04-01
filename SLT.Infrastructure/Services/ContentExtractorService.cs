using HtmlAgilityPack;
using SLT.Core.Interfaces;
using System.Net;
using System.Text.RegularExpressions;

namespace SLT.Infrastructure.Services;

public class ContentExtractorService : IContentExtractorService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ContentExtractorService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ExtractedContent> ExtractAsync(string url)
    {
        var result = new ExtractedContent();

        try
        {
            result.Domain = new Uri(url).Host.Replace("www.", "");

            // Step 1 — Try Jina AI Reader (works with Medium, LinkedIn, any JS site)
            var jinaContent = await ExtractWithJinaAsync(url);
            if (!string.IsNullOrWhiteSpace(jinaContent))
            {
                ParseJinaContent(jinaContent, result, url);

                // If Jina gave us good content, we're done
                if (!string.IsNullOrWhiteSpace(result.RawText) && result.RawText.Length > 100)
                    return result;
            }

            // Step 2 — Fallback to generic HTML scraping
            await ExtractGenericAsync(url, result);
        }
        catch
        {
            // Return whatever was collected
        }

        return result;
    }

    // ─── Jina AI Reader ───────────────────────────────────────────────────────
    private async Task<string> ExtractWithJinaAsync(string url)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Jina");
            var jinaUrl = $"https://r.jina.ai/{url}";

            var request = new HttpRequestMessage(HttpMethod.Get, jinaUrl);
            request.Headers.TryAddWithoutValidation("Accept", "text/plain");
            request.Headers.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36");

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return string.Empty;

            return await response.Content.ReadAsStringAsync();
        }
        catch
        {
            return string.Empty;
        }
    }

    // ─── Parse Jina Markdown Output ───────────────────────────────────────────
    // Jina returns content in this format:
    // Title: <title>
    // URL Source: <url>
    // Author: <author>  (sometimes)
    // Markdown Content: <content>
    private static void ParseJinaContent(string jinaText, ExtractedContent result, string url)
    {
        var lines = jinaText.Split('\n');
        var contentLines = new List<string>();
        var inContent = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("Title:", StringComparison.OrdinalIgnoreCase))
            {
                result.Title = line["Title:".Length..].Trim();
                continue;
            }

            if (line.StartsWith("Author:", StringComparison.OrdinalIgnoreCase))
            {
                result.Author = line["Author:".Length..].Trim();
                continue;
            }

            if (line.StartsWith("URL Source:", StringComparison.OrdinalIgnoreCase))
                continue;

            if (line.StartsWith("Published Time:", StringComparison.OrdinalIgnoreCase))
                continue;

            if (line.StartsWith("Markdown Content:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("================", StringComparison.OrdinalIgnoreCase))
            {
                inContent = true;
                continue;
            }

            if (inContent)
                contentLines.Add(line);
        }

        // If we never found a "Markdown Content:" marker, use everything after line 5
        if (!inContent && lines.Length > 5)
            contentLines.AddRange(lines.Skip(5));

        // Clean markdown syntax from content
        var rawText = string.Join("\n", contentLines);
        rawText = Regex.Replace(rawText, @"\[([^\]]+)\]\([^\)]+\)", "$1"); // links
        rawText = Regex.Replace(rawText, @"#{1,6}\s*", "");                // headings
        rawText = Regex.Replace(rawText, @"\*{1,3}([^\*]+)\*{1,3}", "$1"); // bold/italic
        rawText = Regex.Replace(rawText, @"`{1,3}[^`]*`{1,3}", "");        // code
        rawText = Regex.Replace(rawText, @"\s+", " ").Trim();

        result.RawText = rawText.Length > 4000 ? rawText[..4000] : rawText;

        // Try to extract thumbnail from og:image if not set
        // Jina doesn't provide images, so leave ThumbnailUrl as null
    }

    // ─── Generic HTML Fallback ────────────────────────────────────────────────
    private async Task<ExtractedContent> ExtractGenericAsync(string url, ExtractedContent result)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Extractor");
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36");
            request.Headers.TryAddWithoutValidation("Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            HttpResponseMessage response;
            try { response = await client.SendAsync(request); }
            catch { return result; }

            if (!response.IsSuccessStatusCode) return result;

            var html = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove noise
            var noiseNodes = doc.DocumentNode.SelectNodes(
                "//script|//style|//nav|//footer|//header|//aside|//iframe|//noscript");
            if (noiseNodes != null)
                foreach (var node in noiseNodes.ToList())
                    node.Remove();

            if (string.IsNullOrEmpty(result.Title))
                result.Title = WebUtility.HtmlDecode(
                    GetMetaContent(doc, "og:title") ??
                    GetMetaContent(doc, "twitter:title") ??
                    doc.DocumentNode.SelectSingleNode("//h1")?.InnerText?.Trim() ??
                    doc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim() ??
                    string.Empty).Trim();

            result.Author ??=
                GetMetaContent(doc, "author") ??
                GetMetaContent(doc, "article:author") ??
                doc.DocumentNode.SelectSingleNode("//*[contains(@class,'author')]")?.InnerText?.Trim();

            result.Description =
                GetMetaContent(doc, "og:description") ??
                GetMetaContent(doc, "description") ??
                GetMetaContent(doc, "twitter:description");

            result.ThumbnailUrl ??=
                GetMetaContent(doc, "og:image") ??
                GetMetaContent(doc, "twitter:image");

            if (string.IsNullOrWhiteSpace(result.RawText))
                result.RawText = ExtractBestText(doc);
        }
        catch { }

        return result;
    }

    private static string? GetMetaContent(HtmlDocument doc, string name)
    {
        var node =
            doc.DocumentNode.SelectSingleNode($"//meta[@name='{name}']") ??
            doc.DocumentNode.SelectSingleNode($"//meta[@property='{name}']");
        return node?.GetAttributeValue("content", null);
    }

    private static string ExtractBestText(HtmlDocument doc)
    {
        var selectors = new[]
        {
            "//article", "//main",
            "//*[contains(@class,'post-content')]",
            "//*[contains(@class,'entry-content')]",
            "//*[contains(@class,'article-body')]",
            "//*[contains(@class,'article-content')]",
            "//*[contains(@id,'article')]",
            "//*[contains(@id,'content')]",
        };

        foreach (var selector in selectors)
        {
            var node = doc.DocumentNode.SelectSingleNode(selector);
            if (node != null)
            {
                var text = CleanText(node.InnerText);
                if (text.Length > 200) return Truncate(text, 4000);
            }
        }

        var divs = doc.DocumentNode.SelectNodes("//div") ?? Enumerable.Empty<HtmlNode>();
        HtmlNode? bestDiv = null;
        int bestScore = 0;
        foreach (var div in divs)
        {
            var paragraphs = div.SelectNodes(".//p");
            if (paragraphs == null) continue;
            var score = paragraphs.Sum(p => p.InnerText.Length);
            if (score > bestScore) { bestScore = score; bestDiv = div; }
        }
        if (bestDiv != null)
        {
            var text = CleanText(bestDiv.InnerText);
            if (text.Length > 200) return Truncate(text, 4000);
        }

        var allP = doc.DocumentNode.SelectNodes("//p");
        if (allP != null)
        {
            var joined = string.Join(" ", allP
                .Select(p => p.InnerText.Trim())
                .Where(t => t.Length > 40));
            var text = CleanText(joined);
            if (text.Length > 100) return Truncate(text, 4000);
        }

        var body = doc.DocumentNode.SelectSingleNode("//body");
        return Truncate(CleanText(body?.InnerText ?? string.Empty), 4000);
    }

    private static string CleanText(string text)
    {
        text = WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, @"\s+", " ");
        return text.Trim();
    }

    private static string Truncate(string text, int max) =>
        text.Length > max ? text[..max] : text;
}