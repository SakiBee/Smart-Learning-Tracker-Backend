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
public class RecommendationsController : ControllerBase
{
    private readonly IRepository<LearningEntry> _entryRepo;

    public RecommendationsController(IRepository<LearningEntry> entryRepo)
    {
        _entryRepo = entryRepo;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("entry/{entryId:guid}")]
    public async Task<IActionResult> GetForEntry(Guid entryId)
    {
        var allEntries = (await _entryRepo.ListAsync(
            new EntriesByUserSpec(CurrentUserId))).ToList();

        var target = allEntries.FirstOrDefault(e => e.Id == entryId);
        if (target == null) return NotFound();

        var targetTags = target.Tags.Select(t => t.Name).ToHashSet();
        if (!targetTags.Any()) return Ok(new List<RecommendationDto>());

        var result = allEntries
            .Where(e => e.Id != entryId)
            .Select(e =>
            {
                var matched = targetTags
                    .Intersect(e.Tags.Select(t => t.Name)).ToList();
                return new { Entry = e, MatchedTags = matched, Score = matched.Count };
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
            }).ToList();

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetGlobal()
    {
        var allEntries = (await _entryRepo.ListAsync(
            new EntriesByUserSpec(CurrentUserId))).ToList();

        var topTags = allEntries
            .SelectMany(e => e.Tags.Select(t => t.Name))
            .GroupBy(t => t)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToHashSet();

        if (!topTags.Any()) return Ok(new List<RecommendationDto>());

        var result = allEntries
            .Where(e => !e.IsRead)
            .Select(e =>
            {
                var matched = topTags
                    .Intersect(e.Tags.Select(t => t.Name)).ToList();
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
            }).ToList();

        return Ok(result);
    }
}