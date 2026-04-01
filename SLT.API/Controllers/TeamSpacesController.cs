using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SLT.Application.DTOs;
using SLT.Core.Entities;
using SLT.Core.Interfaces;
using SLT.Infrastructure.Data;

namespace SLT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TeamSpacesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILearningEntryRepository _entryRepo;

    public TeamSpacesController(AppDbContext context, ILearningEntryRepository entryRepo)
    {
        _context = context;
        _entryRepo = entryRepo;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET my team spaces
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var memberships = await _context.TeamMembers
            .Include(m => m.TeamSpace)
                .ThenInclude(t => t.Members)
                    .ThenInclude(m => m.User)
            .Include(m => m.TeamSpace)
                .ThenInclude(t => t.SharedEntries)
            .Where(m => m.UserId == CurrentUserId)
            .ToListAsync();

        var result = memberships.Select(m => MapToDto(m.TeamSpace, CurrentUserId));
        return Ok(result);
    }

    // GET single team
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var team = await _context.TeamSpaces
            .Include(t => t.Members).ThenInclude(m => m.User)
            .Include(t => t.SharedEntries).ThenInclude(e => e.LearningEntry).ThenInclude(le => le.Tags)
            .Include(t => t.SharedEntries).ThenInclude(e => e.SharedByUser)
            .Include(t => t.SharedEntries).ThenInclude(e => e.Comments).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null) return NotFound();

        var isMember = team.Members.Any(m => m.UserId == CurrentUserId);
        if (!isMember) return Forbid();

        return Ok(new
        {
            team = MapToDto(team, CurrentUserId),
            entries = team.SharedEntries
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => MapEntryToDto(e))
                .ToList()
        });
    }

    // POST create team space
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

        _context.TeamSpaces.Add(team);

        // Add creator as owner member
        _context.TeamMembers.Add(new TeamMember
        {
            TeamSpaceId = team.Id,
            UserId = CurrentUserId,
            Role = TeamRole.Owner
        });

        await _context.SaveChangesAsync();
        return Ok(new { id = team.Id, inviteCode = team.InviteCode });
    }

    // POST join by invite code
    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinTeamDto dto)
    {
        var team = await _context.TeamSpaces
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.InviteCode == dto.InviteCode.Trim().ToUpper());

        if (team == null)
            return NotFound(new { message = "Invalid invite code." });

        var alreadyMember = team.Members.Any(m => m.UserId == CurrentUserId);
        if (alreadyMember)
            return Conflict(new { message = "You are already a member." });

        _context.TeamMembers.Add(new TeamMember
        {
            TeamSpaceId = team.Id,
            UserId = CurrentUserId,
            Role = TeamRole.Member
        });

        await _context.SaveChangesAsync();
        return Ok(new { message = $"Joined {team.Name}!", teamId = team.Id });
    }

    // POST regenerate invite code
    [HttpPost("{id:guid}/regenerate-code")]
    public async Task<IActionResult> RegenerateCode(Guid id)
    {
        var team = await _context.TeamSpaces.FindAsync(id);
        if (team == null || team.OwnerId != CurrentUserId) return Forbid();

        team.InviteCode = GenerateInviteCode();
        await _context.SaveChangesAsync();

        return Ok(new { inviteCode = team.InviteCode });
    }

    // POST share entry to team
    [HttpPost("{id:guid}/entries")]
    public async Task<IActionResult> ShareEntry(Guid id, [FromBody] ShareEntryToTeamDto dto)
    {
        var isMember = await _context.TeamMembers
            .AnyAsync(m => m.TeamSpaceId == id && m.UserId == CurrentUserId);
        if (!isMember) return Forbid();

        var entry = await _entryRepo.GetByIdAsync(dto.LearningEntryId);
        if (entry == null || entry.UserId != CurrentUserId)
            return NotFound(new { message = "Entry not found." });

        var alreadyShared = await _context.TeamEntries
            .AnyAsync(te => te.TeamSpaceId == id && te.LearningEntryId == dto.LearningEntryId);
        if (alreadyShared)
            return Conflict(new { message = "Already shared to this team." });

        var teamEntry = new TeamEntry
        {
            TeamSpaceId = id,
            LearningEntryId = dto.LearningEntryId,
            SharedByUserId = CurrentUserId,
            SharedNote = dto.SharedNote?.Trim()
        };

        _context.TeamEntries.Add(teamEntry);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Shared to team!" });
    }

    // DELETE remove entry from team
    [HttpDelete("{id:guid}/entries/{entryId:guid}")]
    public async Task<IActionResult> RemoveEntry(Guid id, Guid entryId)
    {
        var teamEntry = await _context.TeamEntries
            .FirstOrDefaultAsync(te => te.Id == entryId && te.TeamSpaceId == id);

        if (teamEntry == null) return NotFound();

        var isOwner = await _context.TeamSpaces
            .AnyAsync(t => t.Id == id && t.OwnerId == CurrentUserId);

        if (teamEntry.SharedByUserId != CurrentUserId && !isOwner)
            return Forbid();

        _context.TeamEntries.Remove(teamEntry);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // POST add comment
    [HttpPost("{id:guid}/entries/{entryId:guid}/comments")]
    public async Task<IActionResult> AddComment(Guid id, Guid entryId, [FromBody] AddCommentDto dto)
    {
        var isMember = await _context.TeamMembers
            .AnyAsync(m => m.TeamSpaceId == id && m.UserId == CurrentUserId);
        if (!isMember) return Forbid();

        var teamEntry = await _context.TeamEntries.FindAsync(entryId);
        if (teamEntry == null) return NotFound();

        var user = await _context.Users.FindAsync(CurrentUserId);

        var comment = new EntryComment
        {
            Text = dto.Text.Trim(),
            TeamEntryId = entryId,
            UserId = CurrentUserId
        };

        _context.EntryComments.Add(comment);
        await _context.SaveChangesAsync();

        return Ok(new CommentDto
        {
            Id = comment.Id,
            Text = comment.Text,
            UserName = user?.FullName ?? "Unknown",
            UserId = comment.UserId,
            CreatedAt = comment.CreatedAt
        });
    }

    // DELETE remove member
    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        var team = await _context.TeamSpaces.FindAsync(id);
        if (team == null) return NotFound();

        if (team.OwnerId != CurrentUserId && userId != CurrentUserId)
            return Forbid();

        var member = await _context.TeamMembers
            .FirstOrDefaultAsync(m => m.TeamSpaceId == id && m.UserId == userId);

        if (member == null) return NotFound();

        _context.TeamMembers.Remove(member);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE team space (owner only)
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var team = await _context.TeamSpaces.FindAsync(id);
        if (team == null || team.OwnerId != CurrentUserId)
            return Forbid();

        _context.TeamSpaces.Remove(team);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
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