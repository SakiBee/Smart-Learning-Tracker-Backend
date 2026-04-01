using SLT.Core.Enums;

namespace SLT.Application.DTOs;

public class CreateLearningEntryDto
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Summary { get; set; }
    public string? PersonalNotes { get; set; }
    public ContentType ContentType { get; set; } = ContentType.Article;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public bool IsReadLater { get; set; } = false;
    public List<string> Tags { get; set; } = new();
}
public class LearningEntryDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Summary { get; set; }
    public string? KeyPoints { get; set; }
    public string? PersonalNotes { get; set; }
    public ContentType ContentType { get; set; }
    public PriorityLevel Priority { get; set; }
    public bool IsReadLater { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsRead { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class UpdateLearningEntryDto
{
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? PersonalNotes { get; set; }
    public PriorityLevel? Priority { get; set; }
    public bool? IsReadLater { get; set; }
    public bool? IsFavorite { get; set; }
    public bool? IsRead { get; set; }
    public List<string>? Tags { get; set; }
}