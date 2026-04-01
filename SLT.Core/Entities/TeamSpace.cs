namespace SLT.Core.Entities;

public class TeamSpace : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Emoji { get; set; } = "🏢";
    public string InviteCode { get; set; } = string.Empty;

    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<TeamEntry> SharedEntries { get; set; } = new List<TeamEntry>();
}