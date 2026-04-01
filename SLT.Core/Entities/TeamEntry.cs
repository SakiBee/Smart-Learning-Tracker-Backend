namespace SLT.Core.Entities;

public class TeamEntry : BaseEntity
{
    public Guid TeamSpaceId { get; set; }
    public TeamSpace TeamSpace { get; set; } = null!;

    public Guid LearningEntryId { get; set; }
    public LearningEntry LearningEntry { get; set; } = null!;

    public Guid SharedByUserId { get; set; }
    public User SharedByUser { get; set; } = null!;

    public string? SharedNote { get; set; }

    public ICollection<EntryComment> Comments { get; set; } = new List<EntryComment>();
}