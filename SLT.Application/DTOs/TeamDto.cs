namespace SLT.Application.DTOs;

public class TeamSpaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Emoji { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public int MemberCount { get; set; }
    public int EntryCount { get; set; }
    public List<TeamMemberDto> Members { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class TeamMemberDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class TeamEntryDto
{
    public Guid Id { get; set; }
    public Guid LearningEntryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? SharedNote { get; set; }
    public string SharedByName { get; set; } = string.Empty;
    public List<CommentDto> Comments { get; set; } = new();
    public DateTime SharedAt { get; set; }
}

public class CommentDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTeamSpaceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Emoji { get; set; } = "🏢";
}

public class JoinTeamDto
{
    public string InviteCode { get; set; } = string.Empty;
}

public class ShareEntryToTeamDto
{
    public Guid LearningEntryId { get; set; }
    public string? SharedNote { get; set; }
}

public class AddCommentDto
{
    public string Text { get; set; } = string.Empty;
}