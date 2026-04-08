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
public class CollectionsController : ControllerBase
{
    private readonly IRepository<Collection> _collectionRepo;
    private readonly IRepository<CollectionEntry> _collectionEntryRepo;
    private readonly IRepository<LearningEntry> _entryRepo;

    public CollectionsController(
        IRepository<Collection> collectionRepo,
        IRepository<CollectionEntry> collectionEntryRepo,
        IRepository<LearningEntry> entryRepo)
    {
        _collectionRepo = collectionRepo;
        _collectionEntryRepo = collectionEntryRepo;
        _entryRepo = entryRepo;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var collections = await _collectionRepo.ListAsync(
            new CollectionsByUserSpec(CurrentUserId));
        return Ok(collections.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var collection = await _collectionRepo.GetEntityWithSpec(
            new CollectionWithEntriesSpec(id));

        if (collection == null || collection.UserId != CurrentUserId)
            return NotFound();

        return Ok(MapToDto(collection));
    }

    [HttpGet("shared/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetShared(string slug)
    {
        var collection = await _collectionRepo.GetEntityWithSpec(
            new PublicCollectionBySlugSpec(slug));

        if (collection == null) return NotFound();
        return Ok(MapToDto(collection));
    }

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

        return CreatedAtAction(nameof(GetById),
            new { id = collection.Id }, MapToDto(collection));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateCollectionDto dto)
    {
        var collection = await _collectionRepo.GetEntityWithSpec(
            new CollectionByIdAndUserSpec(id, CurrentUserId));

        if (collection == null) return NotFound();

        if (dto.Name != null) collection.Name = dto.Name.Trim();
        if (dto.Description != null) collection.Description = dto.Description.Trim();
        if (dto.Emoji != null) collection.Emoji = dto.Emoji;

        if (dto.IsPublic.HasValue)
        {
            collection.IsPublic = dto.IsPublic.Value;
            if (dto.IsPublic.Value && string.IsNullOrEmpty(collection.ShareSlug))
                collection.ShareSlug = GenerateSlug(collection.Name);
        }

        collection.UpdatedAt = DateTime.UtcNow;
        _collectionRepo.Update(collection);
        await _collectionRepo.SaveChangesAsync();

        return Ok(MapToDto(collection));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var collection = await _collectionRepo.GetEntityWithSpec(
            new CollectionByIdAndUserSpec(id, CurrentUserId));

        if (collection == null) return NotFound();

        _collectionRepo.Remove(collection);
        await _collectionRepo.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:guid}/entries")]
    public async Task<IActionResult> AddEntry(
        Guid id, [FromBody] AddToCollectionDto dto)
    {
        var collection = await _collectionRepo.GetEntityWithSpec(
            new CollectionByIdAndUserSpec(id, CurrentUserId));

        if (collection == null) return NotFound();

        var entry = await _entryRepo.GetEntityWithSpec(
            new EntryByIdAndUserSpec(dto.LearningEntryId, CurrentUserId));

        if (entry == null)
            return NotFound(new { message = "Entry not found." });

        var exists = await _collectionEntryRepo.AnyAsync(
            new CollectionEntryExistsSpec(id, dto.LearningEntryId));

        if (exists)
            return Conflict(new { message = "Entry already in this collection." });

        await _collectionEntryRepo.AddAsync(new CollectionEntry
        {
            CollectionId = id,
            LearningEntryId = dto.LearningEntryId
        });

        await _collectionEntryRepo.SaveChangesAsync();
        return Ok(new { message = "Added to collection." });
    }

    [HttpDelete("{id:guid}/entries/{entryId:guid}")]
    public async Task<IActionResult> RemoveEntry(Guid id, Guid entryId)
    {
        var collectionEntry = await _collectionEntryRepo.GetEntityWithSpec(
            new CollectionEntryByIdsSpec(id, entryId));

        if (collectionEntry == null) return NotFound();

        _collectionEntryRepo.Remove(collectionEntry);
        await _collectionEntryRepo.SaveChangesAsync();
        return NoContent();
    }

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

    private static string GenerateSlug(string name) =>
        $"{name.ToLower().Replace(" ", "-").Replace("'", "")}-{Guid.NewGuid().ToString()[..6]}";
}