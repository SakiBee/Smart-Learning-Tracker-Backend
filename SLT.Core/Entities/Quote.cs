namespace SLT.Core.Entities;

public class Quote : BaseEntity
{
    public string Text { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? Color { get; set; } = "yellow";

    public Guid LearningEntryId { get; set; }
    public LearningEntry LearningEntry { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}