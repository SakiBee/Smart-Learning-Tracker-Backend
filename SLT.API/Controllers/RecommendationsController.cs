using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLT.Application.DTOs;
using SLT.Core.Interfaces;

namespace SLT.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly ILearningEntryRepository _entryRepo;

    public RecommendationsController(ILearningEntryRepository entryRepo)
    {
        _entryRepo = entryRepo;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET recommendations for a specific entry
    [HttpGet("entry/{entryId:guid}")]
    public async Task<IActionResult> GetForEntry(Guid entryId)
    {
        var allEntries = (await _entryRepo.GetByUserIdAsync(CurrentUserId)).ToList();
        var target = allEntries.FirstOrDefault(e => e.Id == entryId);

        if (target == null) return NotFound();

        var targetTags = target.Tags.Select(t => t.Name).ToHashSet();
        if (!targetTags.Any())
            return Ok(new List<RecommendationDto>());

        var recommendations = allEntries
            .Where(e => e.Id != entryId)
            .Select(e =>
            {
                var entryTags = e.Tags.Select(t => t.Name).ToHashSet();
                var matched = targetTags.Intersect(entryTags).ToList();
                return new
                {
                    Entry = e,
                    MatchedTags = matched,
                    Score = matched.Count
                };
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(5)
            .Select(x => new RecommendationDto
            {
                Id = x.Entry.Id,
                Title = x.Entry.Title,
                Url = x.Entry.Url,
                Summary = x.Entry.Summary,
                Tags = x.Entry.Tags.Select(t => t.Name).ToList(),
                MatchScore = x.Score,
                MatchedTags = x.MatchedTags,
                CreatedAt = x.Entry.CreatedAt,
            })
            .ToList();

        return Ok(recommendations);
    }

    // GET global recommendations based on most used tags
    [HttpGet]
    public async Task<IActionResult> GetGlobal()
    {
        var allEntries = (await _entryRepo.GetByUserIdAsync(CurrentUserId)).ToList();

        // Find top tags
        var topTags = allEntries
            .SelectMany(e => e.Tags.Select(t => t.Name))
            .GroupBy(t => t)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToHashSet();

        if (!topTags.Any())
            return Ok(new List<RecommendationDto>());

        // Recommend unread entries that match top tags
        var recommendations = allEntries
            .Where(e => !e.IsRead)
            .Select(e =>
            {
                var entryTags = e.Tags.Select(t => t.Name).ToHashSet();
                var matched = topTags.Intersect(entryTags).ToList();
                return new { Entry = e, MatchedTags = matched, Score = matched.Count };
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Entry.Priority)
            .Take(6)
            .Select(x => new RecommendationDto
            {
                Id = x.Entry.Id,
                Title = x.Entry.Title,
                Url = x.Entry.Url,
                Summary = x.Entry.Summary,
                Tags = x.Entry.Tags.Select(t => t.Name).ToList(),
                MatchScore = x.Score,
                MatchedTags = x.MatchedTags,
                CreatedAt = x.Entry.CreatedAt,
            })
            .ToList();

        return Ok(recommendations);
    }
}