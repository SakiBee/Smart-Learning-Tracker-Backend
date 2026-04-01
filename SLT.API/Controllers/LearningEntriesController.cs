using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLT.Application.DTOs;
using SLT.Core.Entities;
using SLT.Core.Interfaces;

namespace SLT.API.Controllers;

[ApiController]
[Authorize]                          // 👈 Protect all endpoints
[Route("api/[controller]")]
public class LearningEntriesController : ControllerBase
{
    private readonly ILearningEntryRepository _repository;

    public LearningEntriesController(ILearningEntryRepository repository)
    {
        _repository = repository;
    }

    // 👇 Replaces the hardcoded _tempUserId
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entries = await _repository.GetByUserIdAsync(CurrentUserId);
        var result = entries.Select(MapToDto);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var entry = await _repository.GetByIdWithTagsAsync(id);
        if (entry == null || entry.UserId != CurrentUserId)
            return NotFound();

        return Ok(MapToDto(entry));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLearningEntryDto dto)
    {
        var urlExists = await _repository.UrlExistsForUserAsync(dto.Url, CurrentUserId);
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
            UserId = CurrentUserId       // 👈 Real user from JWT
        };

        await _repository.AddAsync(entry);
        await _repository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, MapToDto(entry));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLearningEntryDto dto)
    {
        var entry = await _repository.GetByIdWithTagsAsync(id);
        if (entry == null || entry.UserId != CurrentUserId)
            return NotFound();

        if (dto.Title != null) entry.Title = dto.Title;
        if (dto.Summary != null) entry.Summary = dto.Summary;
        if (dto.PersonalNotes != null) entry.PersonalNotes = dto.PersonalNotes;
        if (dto.Priority.HasValue) entry.Priority = dto.Priority.Value;
        if (dto.IsReadLater.HasValue) entry.IsReadLater = dto.IsReadLater.Value;
        if (dto.IsFavorite.HasValue) entry.IsFavorite = dto.IsFavorite.Value;
        if (dto.IsRead.HasValue) entry.IsRead = dto.IsRead.Value;

        _repository.Update(entry);
        await _repository.SaveChangesAsync();

        return Ok(MapToDto(entry));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entry = await _repository.GetByIdAsync(id);
        if (entry == null || entry.UserId != CurrentUserId)
            return NotFound();

        _repository.Remove(entry);
        await _repository.SaveChangesAsync();

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