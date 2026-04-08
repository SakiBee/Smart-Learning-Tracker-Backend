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
public class QuotesController : ControllerBase
{
    private readonly IRepository<Quote> _quoteRepo;
    private readonly IRepository<LearningEntry> _entryRepo;

    public QuotesController(
        IRepository<Quote> quoteRepo,
        IRepository<LearningEntry> entryRepo)
    {
        _quoteRepo = quoteRepo;
        _entryRepo = entryRepo;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var quotes = await _quoteRepo.ListAsync(
            new QuotesByUserSpec(CurrentUserId));
        return Ok(quotes.Select(MapToDto));
    }

    [HttpGet("entry/{entryId:guid}")]
    public async Task<IActionResult> GetByEntry(Guid entryId)
    {
        var quotes = await _quoteRepo.ListAsync(
            new QuotesByEntrySpec(entryId, CurrentUserId));
        return Ok(quotes.Select(MapToDto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuoteDto dto)
    {
        var entry = await _entryRepo.GetEntityWithSpec(
            new EntryByIdAndUserSpec(dto.LearningEntryId, CurrentUserId));

        if (entry == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Text))
            return BadRequest(new { message = "Quote text is required." });

        var quote = new Quote
        {
            Text = dto.Text.Trim(),
            Note = dto.Note?.Trim(),
            Color = dto.Color ?? "yellow",
            LearningEntryId = dto.LearningEntryId,
            UserId = CurrentUserId
        };

        await _quoteRepo.AddAsync(quote);
        await _quoteRepo.SaveChangesAsync();

        return Ok(MapToDto(quote));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateQuoteDto dto)
    {
        var quote = await _quoteRepo.GetEntityWithSpec(
            new QuoteByIdAndUserSpec(id, CurrentUserId));

        if (quote == null) return NotFound();

        if (dto.Note != null) quote.Note = dto.Note.Trim();
        if (dto.Color != null) quote.Color = dto.Color;
        quote.UpdatedAt = DateTime.UtcNow;

        _quoteRepo.Update(quote);
        await _quoteRepo.SaveChangesAsync();

        return Ok(MapToDto(quote));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var quote = await _quoteRepo.GetEntityWithSpec(
            new QuoteByIdAndUserSpec(id, CurrentUserId));

        if (quote == null) return NotFound();

        _quoteRepo.Remove(quote);
        await _quoteRepo.SaveChangesAsync();
        return NoContent();
    }

    private static QuoteDto MapToDto(Quote q) => new()
    {
        Id = q.Id,
        Text = q.Text,
        Note = q.Note,
        Color = q.Color,
        LearningEntryId = q.LearningEntryId,
        EntryTitle = q.LearningEntry?.Title ?? "",
        EntryUrl = q.LearningEntry?.Url ?? "",
        CreatedAt = q.CreatedAt
    };
}