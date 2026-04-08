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
public class LearningEntriesController : ControllerBase
{
    private readonly IRepository<LearningEntry> _entryRepo;

    public LearningEntriesController(IRepository<LearningEntry> entryRepo)
    {
        _entryRepo = entryRepo;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entries = await _entryRepo.ListAsync(
            new EntriesByUserSpec(CurrentUserId));
        return Ok(entries.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var entry = await _entryRepo.GetEntityWithSpec(
            new EntryByIdAndUserSpec(id, CurrentUserId));

        if (entry == null) return NotFound();
        return Ok(MapToDto(entry));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLearningEntryDto dto)
    {
        var urlExists = await _entryRepo.AnyAsync(
            new EntryUrlExistsSpec(dto.Url, CurrentUserId));

        if (urlExists)
            return Conflict(new { message = "This URL has already been saved." });

        var entry = new LearningEntry
        {
            Url = dto.Url,
            Title = dto.Title,
            Author = dto.Author,
            ThumbnailUrl = dto.ThumbnailUrl,
            Summary = dto.Summary,
            PersonalNotes = dto.PersonalNotes,
            ContentType = dto.ContentType,
            Priority = dto.Priority,
            IsReadLater = dto.IsReadLater,
            UserId = CurrentUserId
        };

        await _entryRepo.AddAsync(entry);
        await _entryRepo.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById),
            new { id = entry.Id }, MapToDto(entry));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateLearningEntryDto dto)
    {
        var entry = await _entryRepo.GetEntityWithSpec(
            new EntryByIdAndUserSpec(id, CurrentUserId));

        if (entry == null) return NotFound();

        if (dto.Title != null) entry.Title = dto.Title;
        if (dto.Summary != null) entry.Summary = dto.Summary;
        if (dto.PersonalNotes != null) entry.PersonalNotes = dto.PersonalNotes;
        if (dto.Priority.HasValue) entry.Priority = dto.Priority.Value;
        if (dto.IsReadLater.HasValue) entry.IsReadLater = dto.IsReadLater.Value;
        if (dto.IsFavorite.HasValue) entry.IsFavorite = dto.IsFavorite.Value;
        if (dto.IsRead.HasValue) entry.IsRead = dto.IsRead.Value;
        entry.UpdatedAt = DateTime.UtcNow;

        _entryRepo.Update(entry);
        await _entryRepo.SaveChangesAsync();

        return Ok(MapToDto(entry));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entry = await _entryRepo.GetEntityWithSpec(
            new EntryByIdAndUserSpec(id, CurrentUserId));

        if (entry == null) return NotFound();

        _entryRepo.Remove(entry);
        await _entryRepo.SaveChangesAsync();

        return NoContent();
    }

    private static LearningEntryDto MapToDto(LearningEntry entry) => new()
    {
        Id = entry.Id,
        Url = entry.Url,
        Title = entry.Title,
        Author = entry.Author,
        ThumbnailUrl = entry.ThumbnailUrl,
        Summary = entry.Summary,
        KeyPoints = entry.KeyPoints,
        PersonalNotes = entry.PersonalNotes,
        ContentType = entry.ContentType,
        Priority = entry.Priority,
        IsReadLater = entry.IsReadLater,
        IsFavorite = entry.IsFavorite,
        IsRead = entry.IsRead,
        Tags = entry.Tags?.Select(t => t.Name).ToList() ?? new(),
        CreatedAt = entry.CreatedAt
    };
}