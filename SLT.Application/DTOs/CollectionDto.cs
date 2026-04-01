namespace SLT.Application.DTOs;

public class CollectionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Emoji { get; set; }
    public bool IsPublic { get; set; }
    public string? ShareSlug { get; set; }
    public int EntryCount { get; set; }
    public List<CollectionEntryDto> Entries { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CollectionEntryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCollectionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Emoji { get; set; } = "📁";
}

public class UpdateCollectionDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Emoji { get; set; }
    public bool? IsPublic { get; set; }
}

public class AddToCollectionDto
{
    public Guid LearningEntryId { get; set; }
}

public class RecommendationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public List<string> Tags { get; set; } = new();
    public int MatchScore { get; set; }
    public List<string> MatchedTags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}