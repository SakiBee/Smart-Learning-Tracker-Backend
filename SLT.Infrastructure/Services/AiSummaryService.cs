using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SLT.Core.Interfaces;

namespace SLT.Infrastructure.Services;

public class AiSummaryService : IAiSummaryService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AiSummaryService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("Groq");
        _configuration = configuration;
    }

    public async Task<AiSummaryResult> SummarizeAsync(string title, string content, string url)
    {
        var apiKey = _configuration["Groq:ApiKey"]!;
        var model = _configuration["Groq:Model"] ?? "openai/gpt-oss-120b";

        // Derive topic hint from URL slug
var urlSlug = url.Split('/').LastOrDefault()?.Replace("-", " ") ?? "";

var prompt = $$$"""
    You are a smart learning assistant that summarizes online articles and posts.

    You MUST always return a valid JSON response — never ask for more information.

    Here is what I have:
    URL: {url}
    URL Slug (hints at topic): {urlSlug}
    Title: {(string.IsNullOrWhiteSpace(title) ? "Unknown" : title)}
    Extracted Content: {(string.IsNullOrWhiteSpace(content) ? "Not available" : content)}

    Instructions:
    - If content is available, summarize it accurately.
    - If content is NOT available, use the title and URL slug to write a reasonable inferred summary. 
      Start the summary with "Based on the title: " so the user knows it's inferred.
    - Always return 3-4 key points. If inferring, make them based on the title topic.
    - Always return 2-3 lowercase tags relevant to the topic.
    - Pick the best contentType from: Article, LinkedInPost, Tutorial, Thread, Newsletter, Video, Other

    Return ONLY this exact JSON (no markdown, no code fences, no explanation):
    {{
      "summary": "...",
      "keyPoints": ["...", "...", "..."],
      "suggestedTags": ["...", "..."],
      "contentType": "Article"
    }}
    """;

        var requestBody = new
        {
            model,
            max_tokens = 1024,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return new AiSummaryResult { Summary = "Could not generate summary." };

        using var doc = JsonDocument.Parse(responseBody);
        var text = doc.RootElement
            .GetProperty("choices")[0] 
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "{}";

        // Strip markdown code fences if model adds them
        text = text.Trim();
        if (text.StartsWith("```"))
        {
            text = string.Join("\n", text.Split('\n').Skip(1))
                         .Replace("```", "").Trim();
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<AiSummaryResult>(text, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return parsed ?? new AiSummaryResult();
        }
        catch
        {
            return new AiSummaryResult { Summary = text };
        }
    }
}