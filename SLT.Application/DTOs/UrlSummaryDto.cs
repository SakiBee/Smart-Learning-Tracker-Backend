namespace SLT.Application.DTOs;

public class UrlSummaryRequestDto
{
    public string Url { get; set; } = string.Empty;
}

public class UrlSummaryResponseDto
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Description { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<string> KeyPoints { get; set; } = new();
    public List<string> SuggestedTags { get; set; } = new();
    public string ContentType { get; set; } = "Article";
    public string Domain { get; set; } = string.Empty;
}