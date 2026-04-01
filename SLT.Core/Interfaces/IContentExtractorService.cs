namespace SLT.Core.Interfaces;

public class ExtractedContent
{
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string RawText { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
}

public interface IContentExtractorService
{
    Task<ExtractedContent> ExtractAsync(string url);
}