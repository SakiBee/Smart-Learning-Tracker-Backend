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
public class StatsController : ControllerBase
{
    private readonly IRepository<LearningEntry> _entryRepo;

    public StatsController(IRepository<LearningEntry> entryRepo)
    {
        _entryRepo = entryRepo;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var entries = (await _entryRepo.ListAsync(
            new EntriesByUserSpec(CurrentUserId))).ToList();

        var now = DateTime.UtcNow;
        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);

        var stats = new StatsDto
        {
            TotalSaved    = entries.Count,
            TotalRead     = entries.Count(e => e.IsRead),
            TotalFavorites = entries.Count(e => e.IsFavorite),
            TotalReadLater = entries.Count(e => e.IsReadLater),
            SavedThisWeek = entries.Count(e => e.CreatedAt >= startOfWeek),
            ReadThisWeek  = entries.Count(e => e.IsRead && e.UpdatedAt >= startOfWeek),
        };

        // Streak calculation
        var activeDates = entries
            .Select(e => e.CreatedAt.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        var checkDate = now.Date;
        foreach (var date in activeDates)
        {
            if (date == checkDate || date == checkDate.AddDays(-1))
            { stats.CurrentStreak++; checkDate = date; }
            else break;
        }

        var sortedDates = activeDates.OrderBy(d => d).ToList();
        int tempStreak = 0;
        for (int i = 0; i < sortedDates.Count; i++)
        {
            if (i == 0 || sortedDates[i] == sortedDates[i - 1].AddDays(1))
            { tempStreak++; stats.LongestStreak = Math.Max(stats.LongestStreak, tempStreak); }
            else tempStreak = 1;
        }

        stats.Last30DaysActivity = Enumerable.Range(0, 30)
            .Select(i => now.Date.AddDays(-29 + i))
            .Select(date => new DailyActivityDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                Count = entries.Count(e => e.CreatedAt.Date == date)
            }).ToList();

        stats.TopTags = entries
            .SelectMany(e => e.Tags.Select(t => t.Name))
            .GroupBy(t => t)
            .Select(g => new TopTagDto { Tag = g.Key, Count = g.Count() })
            .OrderByDescending(t => t.Count)
            .Take(8).ToList();

        stats.ContentTypeBreakdown = entries
            .GroupBy(e => e.ContentType.ToString())
            .Select(g => new ContentTypeStatDto
            { ContentType = g.Key, Count = g.Count() })
            .OrderByDescending(c => c.Count).ToList();

        stats.WeeklyDigest = entries
            .Where(e => e.CreatedAt >= now.AddDays(-7))
            .OrderByDescending(e => e.CreatedAt)
            .Take(10)
            .Select(e => new WeeklyDigestEntryDto
            {
                Id = e.Id,
                Title = e.Title,
                Url = e.Url,
                Summary = e.Summary,
                Tags = e.Tags.Select(t => t.Name).ToList(),
                IsRead = e.IsRead,
                CreatedAt = e.CreatedAt
            }).ToList();

        return Ok(stats);
    }
}