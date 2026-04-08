using SLT.Core.Entities;

namespace SLT.Core.Specifications;

// All teams a user belongs to (via membership)
public class TeamMembershipsByUserSpec : BaseSpecification<TeamMember>
{
    public TeamMembershipsByUserSpec(Guid userId)
        : base(m => m.UserId == userId)
    {
        AddInclude("TeamSpace.Members.User");
        AddInclude("TeamSpace.SharedEntries");
    }
}

// Team with full details
public class TeamWithDetailsSpec : BaseSpecification<TeamSpace>
{
    public TeamWithDetailsSpec(Guid id)
        : base(t => t.Id == id)
    {
        AddInclude("Members.User");
        AddInclude("SharedEntries.LearningEntry.Tags");
        AddInclude("SharedEntries.SharedByUser");
        AddInclude("SharedEntries.Comments.User");
    }
}

// Team by invite code
public class TeamByInviteCodeSpec : BaseSpecification<TeamSpace>
{
    public TeamByInviteCodeSpec(string code)
        : base(t => t.InviteCode == code.ToUpper())
    {
        AddInclude(t => t.Members);
    }
}

// Check if user is already a member
public class TeamMemberExistsSpec : BaseSpecification<TeamMember>
{
    public TeamMemberExistsSpec(Guid teamId, Guid userId)
        : base(m => m.TeamSpaceId == teamId && m.UserId == userId) { }
}

// Member by team + user IDs
public class TeamMemberByIdsSpec : BaseSpecification<TeamMember>
{
    public TeamMemberByIdsSpec(Guid teamId, Guid userId)
        : base(m => m.TeamSpaceId == teamId && m.UserId == userId) { }
}

// Team entry by team + entry IDs
public class TeamEntryByIdsSpec : BaseSpecification<TeamEntry>
{
    public TeamEntryByIdsSpec(Guid teamId, Guid teamEntryId)
        : base(te => te.TeamSpaceId == teamId && te.Id == teamEntryId) { }
}

// Check if entry already shared to team
public class TeamEntryExistsSpec : BaseSpecification<TeamEntry>
{
    public TeamEntryExistsSpec(Guid teamId, Guid entryId)
        : base(te => te.TeamSpaceId == teamId && te.LearningEntryId == entryId) { }
}

// Team owned by user
public class TeamByIdAndOwnerSpec : BaseSpecification<TeamSpace>
{
    public TeamByIdAndOwnerSpec(Guid id, Guid ownerId)
        : base(t => t.Id == id && t.OwnerId == ownerId) { }
}