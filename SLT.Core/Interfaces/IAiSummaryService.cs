namespace SLT.Core.Interfaces;

public class AiSummaryResult
{
    public string Summary { get; set; } = string.Empty;
    public List<string> KeyPoints { get; set; } = new();
    public List<string> SuggestedTags { get; set; } = new();
    public string ContentType { get; set; } = string.Empty;
}

public interface IAiSummaryService
{
    Task<AiSummaryResult> SummarizeAsync(string title, string content, string url);
}