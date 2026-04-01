namespace SLT.Core.Entities;

public enum TeamRole { Member, Admin, Owner }

public class TeamMember : BaseEntity
{
    public Guid TeamSpaceId { get; set; }
    public TeamSpace TeamSpace { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public TeamRole Role { get; set; } = TeamRole.Member;
}