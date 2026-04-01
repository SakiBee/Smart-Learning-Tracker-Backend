using SLT.Core.Enums;

namespace SLT.Core.Entities;

public class LearningEntry : BaseEntity
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Summary { get; set; }
    public string? KeyPoints { get; set; }        // stored as JSON string
    public string? PersonalNotes { get; set; }
    public ContentType ContentType { get; set; } = ContentType.Article;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public bool IsReadLater { get; set; } = false;
    public bool IsFavorite { get; set; } = false;
    public bool IsRead { get; set; } = false;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}