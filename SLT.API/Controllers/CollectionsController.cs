using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLT.Application.DTOs;
using SLT.Core.Entities;
using SLT.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SLT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionRepository _collectionRepo;
    private readonly ILearningEntryRepository _entryRepo;

    public CollectionsController(
        ICollectionRepository collectionRepo,
        ILearningEntryRepository entryRepo)
    {
        _collectionRepo = collectionRepo;
        _entryRepo = entryRepo;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET all collections
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var collections = await _collectionRepo.GetByUserIdAsync(CurrentUserId);
        return Ok(collections.Select(MapToDto));
    }

    // GET single collection
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var collection = await _collectionRepo.GetByIdWithEntriesAsync(id);
        if (collection == null || collection.UserId != CurrentUserId)
            return NotFound();
        return Ok(MapToDto(collection));
    }

    // GET public collection by share slug (no auth)
    [HttpGet("shared/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetShared(string slug)
    {
        var collection = await _collectionRepo.GetByShareSlugAsync(slug);
        if (collection == null) return NotFound();
        return Ok(MapToDto(collection));
    }

    // POST create collection
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCollectionDto dto)
    {
        var collection = new Collection
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Emoji = dto.Emoji ?? "📁",
            UserId = CurrentUserId
        };

        await _collectionRepo.AddAsync(collection);
        await _collectionRepo.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = collection.Id }, MapToDto(collection));
    }

    // PUT update collection
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCollectionDto dto)
    {
        var collection = await _collectionRepo.GetByIdAsync(id);
        if (collection == null || collection.UserId != CurrentUserId)
            return NotFound();

        if (dto.Name != null) collection.Name = dto.Name.Trim();
        if (dto.Description != null) collection.Description = dto.Description.Trim();
        if (dto.Emoji != null) collection.Emoji = dto.Emoji;

        // Toggle public sharing
        if (dto.IsPublic.HasValue)
        {
            collection.IsPublic = dto.IsPublic.Value;
            if (dto.IsPublic.Value && string.IsNullOrEmpty(collection.ShareSlug))
                collection.ShareSlug = GenerateSlug(collection.Name);
        }

        _collectionRepo.Update(collection);
        await _collectionRepo.SaveChangesAsync();

        return Ok(MapToDto(collection));
    }

    // DELETE collection
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var collection = await _collectionRepo.GetByIdAsync(id);
        if (collection == null || collection.UserId != CurrentUserId)
            return NotFound();

        _collectionRepo.Remove(collection);
        await _collectionRepo.SaveChangesAsync();
        return NoContent();
    }

    // POST add entry to collection
    [HttpPost("{id:guid}/entries")]
    public async Task<IActionResult> AddEntry(Guid id, [FromBody] AddToCollectionDto dto)
    {
        var collection = await _collectionRepo.GetByIdAsync(id);
        if (collection == null || collection.UserId != CurrentUserId)
            return NotFound();

        var entry = await _entryRepo.GetByIdAsync(dto.LearningEntryId);
        if (entry == null || entry.UserId != CurrentUserId)
            return NotFound(new { message = "Entry not found." });

        var exists = await _collectionRepo.EntryExistsInCollectionAsync(id, dto.LearningEntryId);
        if (exists)
            return Conflict(new { message = "Entry already in this collection." });

        var context = HttpContext.RequestServices
            .GetRequiredService<SLT.Infrastructure.Data.AppDbContext>();

        await context.CollectionEntries.AddAsync(new CollectionEntry
        {
            CollectionId = id,
            LearningEntryId = dto.LearningEntryId
        });
        await context.SaveChangesAsync();

        return Ok(new { message = "Added to collection." });
    }

    // DELETE remove entry from collection
    [HttpDelete("{id:guid}/entries/{entryId:guid}")]
    public async Task<IActionResult> RemoveEntry(Guid id, Guid entryId)
    {
        var context = HttpContext.RequestServices
            .GetRequiredService<SLT.Infrastructure.Data.AppDbContext>();

        var collectionEntry = await context.CollectionEntries
            .FirstOrDefaultAsync(ce => ce.CollectionId == id && ce.LearningEntryId == entryId);

        if (collectionEntry == null) return NotFound();

        context.CollectionEntries.Remove(collectionEntry);
        await context.SaveChangesAsync();

        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static CollectionDto MapToDto(Collection c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        Emoji = c.Emoji,
        IsPublic = c.IsPublic,
        ShareSlug = c.ShareSlug,
        EntryCount = c.CollectionEntries?.Count ?? 0,
        CreatedAt = c.CreatedAt,
        Entries = c.CollectionEntries?.Select(ce => new CollectionEntryDto
        {
            Id = ce.LearningEntry.Id,
            Title = ce.LearningEntry.Title,
            Url = ce.LearningEntry.Url,
            Summary = ce.LearningEntry.Summary,
            Tags = ce.LearningEntry.Tags?.Select(t => t.Name).ToList() ?? new(),
            IsRead = ce.LearningEntry.IsRead,
            CreatedAt = ce.LearningEntry.CreatedAt,
        }).ToList() ?? new()
    };

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLower()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "");
        return $"{slug}-{Guid.NewGuid().ToString()[..6]}";
    }
}