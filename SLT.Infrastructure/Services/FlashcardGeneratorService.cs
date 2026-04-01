using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SLT.Core.Interfaces;

namespace SLT.Infrastructure.Services;

public class FlashcardGeneratorService : IFlashcardGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public FlashcardGeneratorService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("Groq");
        _configuration = configuration;
    }

    public async Task<List<GeneratedFlashcard>> GenerateAsync(string title, string summary, string keyPoints)
    {
        var apiKey = _configuration["Groq:ApiKey"]!;
        var model = _configuration["Groq:Model"] ?? "openai/gpt-oss-120b";

        var prompt = $$$"""
            You are a learning assistant. Generate flashcards from this content.

            Title: {title}
            Summary: {summary}
            Key Points: {keyPoints}

            Generate 5 high-quality question-answer flashcards that test understanding.
            Questions should be specific and answers should be concise (1-3 sentences).

            Return ONLY valid JSON array (no markdown, no explanation):
            [
              {{"question": "...", "answer": "..."}},
              {{"question": "...", "answer": "..."}},
              {{"question": "...", "answer": "..."}},
              {{"question": "...", "answer": "..."}},
              {{"question": "...", "answer": "..."}}
            ]
            """;

        var requestBody = new
        {
            model,
            max_tokens = 1500,
            messages = new[] { new { role = "user", content = prompt } }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return new List<GeneratedFlashcard>();

        using var doc = JsonDocument.Parse(body);
        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "[]";

        text = text.Trim();
        if (text.StartsWith("```"))
            text = string.Join("\n", text.Split('\n').Skip(1)).Replace("```", "").Trim();

        try
        {
            var cards = JsonSerializer.Deserialize<List<GeneratedFlashcard>>(text,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return cards ?? new List<GeneratedFlashcard>();
        }
        catch
        {
            return new List<GeneratedFlashcard>();
        }
    }
}