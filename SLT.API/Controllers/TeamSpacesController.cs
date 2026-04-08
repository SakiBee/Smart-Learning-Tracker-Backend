using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLT.Application.DTOs;
using SLT.Core.Entities;
using SLT.Core.Interfaces;
using SLT.Core.Specifications;

namespace SLT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TeamSpacesController : ControllerBase
{
    private readonly IRepository<TeamSpace> _teamRepo;
    private readonly IRepository<TeamMember> _memberRepo;
    private readonly IRepository<TeamEntry> _teamEntryRepo;
    private readonly IRepository<EntryComment> _commentRepo;
    private readonly IRepository<LearningEntry> _entryRepo;
    private readonly IRepository<User> _userRepo;

    public TeamSpacesController(
        IRepository<TeamSpace> teamRepo,
        IRepository<TeamMember> memberRepo,
        IRepository<TeamEntry> teamEntryRepo,
        IRepository<EntryComment> commentRepo,
        IRepository<LearningEntry> entryRepo,
        IRepository<User> userRepo)
    {
        _teamRepo = teamRepo;
        _memberRepo = memberRepo;
        _teamEntryRepo = teamEntryRepo;
        _commentRepo = commentRepo;
        _entryRepo = entryRepo;
        _userRepo = userRepo;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var memberships = await _memberRepo.ListAsync(
            new TeamMembershipsByUserSpec(CurrentUserId));

        var result = memberships.Select(m =>
            MapToDto(m.TeamSpace, CurrentUserId));

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var team = await _teamRepo.GetEntityWithSpec(
            new TeamWithDetailsSpec(id));

        if (team == null) return NotFound();

        var isMember = team.Members.Any(m => m.UserId == CurrentUserId);
        if (!isMember) return Forbid();

        return Ok(new
        {
            team = MapToDto(team, CurrentUserId),
            entries = team.SharedEntries
                .OrderByDescending(e => e.CreatedAt)
                .Select(MapEntryToDto).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTeamSpaceDto dto)
    {
        var team = new TeamSpace
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Emoji = dto.Emoji ?? "🏢",
            OwnerId = CurrentUserId,
            InviteCode = GenerateInviteCode()
        };

        await _teamRepo.AddAsync(team);
        await _teamRepo.SaveChangesAsync();

        await _memberRepo.AddAsync(new TeamMember
        {
            TeamSpaceId = team.Id,
            UserId = CurrentUserId,
            Role = TeamRole.Owner
        });
        await _memberRepo.SaveChangesAsync();

        return Ok(new { id = team.Id, inviteCode = team.InviteCode });
    }

    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinTeamDto dto)
    {
        var team = await _teamRepo.GetEntityWithSpec(
            new TeamByInviteCodeSpec(dto.InviteCode));

        if (team == null)
            return NotFound(new { message = "Invalid invite code." });

        var alreadyMember = await _memberRepo.AnyAsync(
            new TeamMemberExistsSpec(team.Id, CurrentUserId));

        if (alreadyMember)
            return Conflict(new { message = "You are already a member." });

        await _memberRepo.AddAsync(new TeamMember
        {
            TeamSpaceId = team.Id,
            UserId = CurrentUserId,
            Role = TeamRole.Member
        });

        await _memberRepo.SaveChangesAsync();
        return Ok(new { message = $"Joined {team.Name}!", teamId = team.Id });
    }

    [HttpPost("{id:guid}/entries")]
    public async Task<IActionResult> ShareEntry(
        Guid id, [FromBody] ShareEntryToTeamDto dto)
    {
        var isMember = await _memberRepo.AnyAsync(
            new TeamMemberExistsSpec(id, CurrentUserId));
        if (!isMember) return Forbid();

        var entry = await _entryRepo.GetEntityWithSpec(
            new EntryByIdAndUserSpec(dto.LearningEntryId, CurrentUserId));
        if (entry == null) return NotFound(new { message = "Entry not found." });

        var alreadyShared = await _teamEntryRepo.AnyAsync(
            new TeamEntryExistsSpec(id, dto.LearningEntryId));
        if (alreadyShared)
            return Conflict(new { message = "Already shared to this team." });

        await _teamEntryRepo.AddAsync(new TeamEntry
        {
            TeamSpaceId = id,
            LearningEntryId = dto.LearningEntryId,
            SharedByUserId = CurrentUserId,
            SharedNote = dto.SharedNote?.Trim()
        });

        await _teamEntryRepo.SaveChangesAsync();
        return Ok(new { message = "Shared to team!" });
    }

    [HttpDelete("{id:guid}/entries/{teamEntryId:guid}")]
    public async Task<IActionResult> RemoveEntry(Guid id, Guid teamEntryId)
    {
        var teamEntry = await _teamEntryRepo.GetEntityWithSpec(
            new TeamEntryByIdsSpec(id, teamEntryId));

        if (teamEntry == null) return NotFound();

        var isOwner = await _teamRepo.AnyAsync(
            new TeamByIdAndOwnerSpec(id, CurrentUserId));

        if (teamEntry.SharedByUserId != CurrentUserId && !isOwner)
            return Forbid();

        _teamEntryRepo.Remove(teamEntry);
        await _teamEntryRepo.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/entries/{teamEntryId:guid}/comments")]
    public async Task<IActionResult> AddComment(
        Guid id, Guid teamEntryId, [FromBody] AddCommentDto dto)
    {
        var isMember = await _memberRepo.AnyAsync(
            new TeamMemberExistsSpec(id, CurrentUserId));
        if (!isMember) return Forbid();

        var teamEntry = await _teamEntryRepo.GetByIdAsync(teamEntryId);
        if (teamEntry == null) return NotFound();

        var user = await _userRepo.GetByIdAsync(CurrentUserId);

        var comment = new EntryComment
        {
            Text = dto.Text.Trim(),
            TeamEntryId = teamEntryId,
            UserId = CurrentUserId
        };

        await _commentRepo.AddAsync(comment);
        await _commentRepo.SaveChangesAsync();

        return Ok(new CommentDto
        {
            Id = comment.Id,
            Text = comment.Text,
            UserName = user?.FullName ?? "Unknown",
            UserId = comment.UserId,
            CreatedAt = comment.CreatedAt
        });
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        var team = await _teamRepo.GetByIdAsync(id);
        if (team == null) return NotFound();

        if (team.OwnerId != CurrentUserId && userId != CurrentUserId)
            return Forbid();

        var member = await _memberRepo.GetEntityWithSpec(
            new TeamMemberByIdsSpec(id, userId));
        if (member == null) return NotFound();

        _memberRepo.Remove(member);
        await _memberRepo.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var team = await _teamRepo.GetEntityWithSpec(
            new TeamByIdAndOwnerSpec(id, CurrentUserId));
        if (team == null) return Forbid();

        _teamRepo.Remove(team);
        await _teamRepo.SaveChangesAsync();
        return NoContent();
    }

    private static TeamSpaceDto MapToDto(TeamSpace t, Guid currentUserId) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        Emoji = t.Emoji,
        InviteCode = t.InviteCode,
        IsOwner = t.OwnerId == currentUserId,
        MemberCount = t.Members?.Count ?? 0,
        EntryCount = t.SharedEntries?.Count ?? 0,
        CreatedAt = t.CreatedAt,
        Members = t.Members?.Select(m => new TeamMemberDto
        {
            UserId = m.UserId,
            FullName = m.User?.FullName ?? "Unknown",
            Email = m.User?.Email ?? "",
            Role = m.Role.ToString(),
            JoinedAt = m.CreatedAt
        }).ToList() ?? new()
    };

    private static TeamEntryDto MapEntryToDto(TeamEntry te) => new()
    {
        Id = te.Id,
        LearningEntryId = te.LearningEntryId,
        Title = te.LearningEntry?.Title ?? "",
        Url = te.LearningEntry?.Url ?? "",
        Summary = te.LearningEntry?.Summary,
        Tags = te.LearningEntry?.Tags?.Select(t => t.Name).ToList() ?? new(),
        SharedNote = te.SharedNote,
        SharedByName = te.SharedByUser?.FullName ?? "Unknown",
        SharedAt = te.CreatedAt,
        Comments = te.Comments?.Select(c => new CommentDto
        {
            Id = c.Id,
            Text = c.Text,
            UserName = c.User?.FullName ?? "Unknown",
            UserId = c.UserId,
            CreatedAt = c.CreatedAt
        }).OrderBy(c => c.CreatedAt).ToList() ?? new()
    };

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}