namespace SLT.Core.Entities;

public class EntryComment : BaseEntity
{
    public string Text { get; set; } = string.Empty;

    public Guid TeamEntryId { get; set; }
    public TeamEntry TeamEntry { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}