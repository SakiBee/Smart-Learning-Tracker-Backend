namespace SLT.Core.Entities;

public class Collection : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Emoji { get; set; } = "📁";
    public bool IsPublic { get; set; } = false;
    public string? ShareSlug { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<CollectionEntry> CollectionEntries { get; set; } = new List<CollectionEntry>();
}