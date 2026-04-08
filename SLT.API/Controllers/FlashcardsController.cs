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
public class FlashcardsController : ControllerBase
{
    private readonly IRepository<Flashcard> _flashcardRepo;
    private readonly IRepository<LearningEntry> _entryRepo;
    private readonly IFlashcardGeneratorService _generator;

    public FlashcardsController(
        IRepository<Flashcard> flashcardRepo,
        IRepository<LearningEntry> entryRepo,
        IFlashcardGeneratorService generator)
    {
        _flashcardRepo = flashcardRepo;
        _entryRepo = entryRepo;
        _generator = generator;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cards = await _flashcardRepo.ListAsync(
            new FlashcardsByUserSpec(CurrentUserId));
        return Ok(cards.Select(MapToDto));
    }

    [HttpGet("due")]
    public async Task<IActionResult> GetDue()
    {
        var cards = await _flashcardRepo.ListAsync(
            new DueFlashcardsSpec(CurrentUserId));
        return Ok(cards.Select(MapToDto));
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateFlashcardsDto dto)
    {
        var entry = await _entryRepo.GetEntityWithSpec(
            new EntryByIdAndUserSpec(dto.LearningEntryId, CurrentUserId));

        if (entry == null) return NotFound();

        if (string.IsNullOrWhiteSpace(entry.Summary))
            return BadRequest(new { message = "Entry needs a summary to generate flashcards." });

        // Remove existing flashcards for this entry
        var existing = await _flashcardRepo.ListAsync(
            new FlashcardsByEntrySpec(dto.LearningEntryId, CurrentUserId));

        foreach (var card in existing)
            _flashcardRepo.Remove(card);

        var generated = await _generator.GenerateAsync(
            entry.Title, entry.Summary ?? "", entry.KeyPoints ?? "");

        if (!generated.Any())
            return BadRequest(new { message = "Could not generate flashcards." });

        var flashcards = generated.Select(g => new Flashcard
        {
            Question = g.Question,
            Answer = g.Answer,
            LearningEntryId = dto.LearningEntryId,
            UserId = CurrentUserId,
        }).ToList();

        foreach (var card in flashcards)
            await _flashcardRepo.AddAsync(card);

        await _flashcardRepo.SaveChangesAsync();

        return Ok(flashcards.Select(MapToDto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFlashcardDto dto)
    {
        var entry = await _entryRepo.GetEntityWithSpec(
            new EntryByIdAndUserSpec(dto.LearningEntryId, CurrentUserId));

        if (entry == null) return NotFound();

        var card = new Flashcard
        {
            Question = dto.Question,
            Answer = dto.Answer,
            LearningEntryId = dto.LearningEntryId,
            UserId = CurrentUserId,
        };

        await _flashcardRepo.AddAsync(card);
        await _flashcardRepo.SaveChangesAsync();

        return Ok(MapToDto(card));
    }

    [HttpPost("{id:guid}/review")]
    public async Task<IActionResult> Review(
        Guid id, [FromBody] ReviewFlashcardDto dto)
    {
        var card = await _flashcardRepo.GetEntityWithSpec(
            new FlashcardByIdAndUserSpec(id, CurrentUserId));

        if (card == null) return NotFound();

        card.TimesReviewed++;
        card.LastReviewedAt = DateTime.UtcNow;

        if (dto.Rating >= 2)
        {
            card.TimesCorrect++;
            var ease = card.EaseFactor +
                (int)((0.1 - (3 - dto.Rating) * (0.08 + (3 - dto.Rating) * 0.02)) * 1000);
            card.EaseFactor = Math.Max(130, ease);
            card.Interval = card.Interval == 1
                ? 6
                : (int)Math.Round(card.Interval * (card.EaseFactor / 100.0));
            card.NextReviewAt = DateTime.UtcNow.AddDays(card.Interval);
        }
        else
        {
            card.Interval = 1;
            card.NextReviewAt = dto.Rating == 0
                ? DateTime.UtcNow.AddMinutes(10)
                : DateTime.UtcNow.AddDays(1);
        }

        card.UpdatedAt = DateTime.UtcNow;
        _flashcardRepo.Update(card);
        await _flashcardRepo.SaveChangesAsync();

        return Ok(MapToDto(card));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var card = await _flashcardRepo.GetEntityWithSpec(
            new FlashcardByIdAndUserSpec(id, CurrentUserId));

        if (card == null) return NotFound();

        _flashcardRepo.Remove(card);
        await _flashcardRepo.SaveChangesAsync();

        return NoContent();
    }

    private static FlashcardDto MapToDto(Flashcard f) => new()
    {
        Id = f.Id,
        Question = f.Question,
        Answer = f.Answer,
        TimesReviewed = f.TimesReviewed,
        TimesCorrect = f.TimesCorrect,
        NextReviewAt = f.NextReviewAt,
        LastReviewedAt = f.LastReviewedAt,
        Interval = f.Interval,
        LearningEntryId = f.LearningEntryId,
        EntryTitle = f.LearningEntry?.Title ?? "",
        IsDue = f.NextReviewAt <= DateTime.UtcNow,
        AccuracyRate = f.TimesReviewed > 0
            ? Math.Round((double)f.TimesCorrect / f.TimesReviewed * 100, 1)
            : 0,
    };
}