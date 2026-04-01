namespace SLT.Core.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<LearningEntry> LearningEntries { get; set; } = new List<LearningEntry>();
}